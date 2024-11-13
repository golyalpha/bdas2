namespace WebApp.Models;

public class Notification
{
    public int Id { get; set; }
    public required string NotificationType { get; set; }
    public required bool Delivered { get; set; }
    public required Organiser Organiser { get; set; }
    public required Request Request { get; set; }
}
