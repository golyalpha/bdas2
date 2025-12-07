namespace WebApp.Models;

public class EditSubstituteViewModel
{
    public Organiser User { get; set; }
    public int? CurrentSubstituteId { get; set; }
    public List<Organiser> PotentialSubstitutes { get; set; } = new();
}