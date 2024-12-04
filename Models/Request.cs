using System.ComponentModel.DataAnnotations;

namespace WebApp.Models;

public class Request
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Start Time is required")]
    [Display(Name = "Start Time")]
    public DateTime Start { get; set; }

    [Required(ErrorMessage = "End Time is required")]
    [Display(Name = "End Time")]
    public DateTime End { get; set; }

    [Required]
    public Organiser Organiser { get; set; }

    // Mock metoda pro naètení konkrétní žádosti (simulace)
    public static Request GetRequest(int id)
    {
        // Návrat konkrétní žádosti
        return new Request
        {
            Id = id,
            Start = DateTime.Now.AddHours(-1),
            End = DateTime.Now,
            Organiser = new Organiser
                {
                    Id = 1,
                    Name = "John Doe",
                    Email = "john@doe.com"
                }
            };
        }

        // Mock metoda pro naètení seznamu žádostí (simulace)
        public static List<Request> ListRequests()
        {
            // Simulace vrácení nìkolika žádostí
            return new List<Request>
            {
                new Request
                {
                    Id = 1,
                    Start = DateTime.Now.AddHours(-3),
                    End = DateTime.Now.AddHours(-2),
                    Organiser = new Organiser
                    {
                        Id = 1,
                        Name = "John Doe",
                        Email = "john@doe.com"
                    }
                },
                new Request
                {
                    Id = 2,
                    Start = DateTime.Now.AddHours(-4),
                    End = DateTime.Now.AddHours(-3),
                    Organiser = new Organiser
                    {
                        Id = 2,
                        Name = "Jane Doe",
                        Email = "jane@doe.com"
                    }
                }
        };
    }
}