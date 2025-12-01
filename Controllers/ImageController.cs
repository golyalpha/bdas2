using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models;

namespace WebApp.Controllers;

[Authorize]
public class ImageController : Controller
{
    private readonly ILogger<ImageController> _logger;
    private const int MaxImageSizeBytes = 5 * 1024 * 1024; // 5 MB
    private static readonly string[] AllowedContentTypes = { "image/jpeg", "image/png", "image/gif", "image/webp" };

    public ImageController(ILogger<ImageController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Upload obrázku pro místnost
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadRoomImage(int roomId, IFormFile imageFile)
    {
        if (imageFile == null || imageFile.Length == 0)
        {
            TempData["Error"] = "Vyberte platný soubor obrázku.";
            return RedirectToAction("Index", "Room");
        }

        // Validace typu souboru
        if (!AllowedContentTypes.Contains(imageFile.ContentType.ToLower()))
        {
            TempData["Error"] = "Povolené formáty jsou pouze: JPEG, PNG, GIF, WEBP.";
            return RedirectToAction("Index", "Room");
        }

        // Validace velikosti
        if (imageFile.Length > MaxImageSizeBytes)
        {
            TempData["Error"] = "Maximální velikost souboru je 5 MB.";
            return RedirectToAction("Index", "Room");
        }

        try
        {
            // Získáme místnost pro ID organizátora a lokace
            var room = Room.GetRoom(roomId);
            
            using (var memoryStream = new MemoryStream())
            {
                await imageFile.CopyToAsync(memoryStream);
                
                var image = new Image
                {
                    Data = memoryStream.ToArray(),
                    IdRoom = roomId,
                    IdOrganizer = room.Organiser.Id,
                    IdLocation = room.Location.Id
                };

                image.Persist();
            }

            TempData["Success"] = "Obrázek byl úspìšnì nahrán.";
            _logger.LogInformation($"Image uploaded for room {roomId} by user {User.Identity?.Name}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error uploading image for room {roomId}");
            TempData["Error"] = "Chyba pøi nahrávání obrázku.";
        }

        return RedirectToAction("Index", "Room");
    }

    /// <summary>
    /// Zobrazí obrázek místnosti
    /// </summary>
    [AllowAnonymous]
    [HttpGet]
    public IActionResult GetRoomImage(int roomId)
    {
        try
        {
            var image = Image.GetImageByRoom(roomId);
            
            if (image == null || image.Data == null || image.Data.Length == 0)
            {
                // DÙLEŽITÉ: vrátit redirect na SVG
                return Redirect("/images/no-image.svg");
            }

            // Detekce MIME typu
            var contentType = "image/jpeg";
            if (image.Data.Length > 2)
            {
                if (image.Data[0] == 0x89 && image.Data[1] == 0x50) contentType = "image/png";
                else if (image.Data[0] == 0x47 && image.Data[1] == 0x49) contentType = "image/gif";
                else if (image.Data[0] == 0x52 && image.Data[1] == 0x49) contentType = "image/webp";
            }

            Response.Headers.Add("Cache-Control", "public,max-age=3600");
            return File(image.Data, contentType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error loading image for room {roomId}");
            return Redirect("/images/no-image.svg");
        }
    }

    /// <summary>
    /// Smazání obrázku místnosti
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrator")]
    public IActionResult DeleteRoomImage(int roomId)
    {
        try
        {
            var image = Image.GetImageByRoom(roomId);
            if (image != null)
            {
                image.Delete();
                TempData["Success"] = "Obrázek byl úspìšnì smazán.";
                _logger.LogInformation($"Image deleted for room {roomId} by user {User.Identity?.Name}");
            }
            else
            {
                TempData["Warning"] = "Místnost nemá pøiøazený obrázek.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting image for room {roomId}");
            TempData["Error"] = "Chyba pøi mazání obrázku.";
        }

        return RedirectToAction("Index", "Room");
    }
}