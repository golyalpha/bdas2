using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace WebApp.Models;

public class Organiser
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }

    public static Organiser GetOrganiser(int Id)
    {
        throw new NotImplementedException();
    }

    public static List<Organiser> ListOrganisers()
    {
        throw new NotImplementedException();
    }
}
