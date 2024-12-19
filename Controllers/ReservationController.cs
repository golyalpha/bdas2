using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models;

[Authorize]
public class ReservationController : Controller
{
    private readonly ILogger<ReservationController> _logger;

    public ReservationController(ILogger<ReservationController> logger)
    {
        _logger = logger;
    }

    // Index: Zobrazí seznam rezervací
    public IActionResult Index()
    {
        var reservations = Reservation.ListReservations(); // Získání seznamu rezervací
        return View(reservations);
    }

    // GET: Room/Edit/{id}
    [HttpGet]
    [Route("Reservation/Edit/{id?}")]
    [Route("Reservation/Create")]
    public IActionResult Persist(int? id)
        {
        if (id == null || id == 0)
        {
            var newRoom = new Reservation
            {
                Id = 0,
                Start = new DateTime(),
                End = new DateTime(),
                Room = new Room { Id = 0 },
                Organiser = new Organiser { Id = 0 },
            };
            return View("Persist", newRoom); // Načte formulář Persist.cshtml
        }
        else
        {
            // Načítáme existující záznam
            var reservation = Reservation.GetReservation(id.Value);
            if (reservation == null)
            {
                return NotFound();
            }
            return View("Persist", reservation); // Načte formulář Persist.cshtml
        }
    }

    // POST: Reservation/Persist
    [HttpPost]
    public IActionResult Persist(Reservation reservation)
    {
        reservation.Persist();
        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult Delete(int id)
    {
        try
        {
            Reservation reservation = Reservation.GetReservation(id);
            reservation.Delete();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting room.");
        }
        return RedirectToAction("Index");
    }
}
