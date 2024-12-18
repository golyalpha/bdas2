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

    // GET: Reservation/Create
    [HttpGet]
    public IActionResult Create()
    {
        return View(); // Zobrazí formulář Create.cshtml
    }

    // POST: Reservation/Create
    [HttpPost]
    public IActionResult Create(Reservation reservation)
    {
        if (ModelState.IsValid)
        {
            // Logika pro přidání nové rezervace
            _logger.LogInformation("New reservation created successfully.");
            return RedirectToAction("Index");
        }
        return View(reservation);
    }

    // GET: Reservation/Edit/5
    [HttpGet]
    public IActionResult Edit(int id)
    {
        var reservation = Reservation.GetReservation(id); // Získání konkrétní rezervace podle ID
        if (reservation == null)
        {
            return NotFound();
        }
        return View(reservation); // Zobrazí formulář Edit.cshtml s předvyplněnými hodnotami
    }

    // POST: Reservation/Update
    [HttpPost]
    public IActionResult Update(Reservation updatedReservation)
    {
        if (ModelState.IsValid)
        {
            var existingReservation = Reservation.GetReservation(updatedReservation.Id);
            if (existingReservation != null)
            {
                // Aktualizace dat existující rezervace
                existingReservation.Start = updatedReservation.Start;
                existingReservation.End = updatedReservation.End;
                existingReservation.Room = updatedReservation.Room;
                existingReservation.Organiser = updatedReservation.Organiser;

                _logger.LogInformation("Reservation updated successfully.");
                return RedirectToAction("Index");
            }
        }
        return View("Edit", updatedReservation); // V případě chyby se vrátí na editovací stránku
    }
}
