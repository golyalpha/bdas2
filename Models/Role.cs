namespace WebApp.Models;

public class Role
{
    public int Id { get; set; }
    public required string Name { get; set; }

    public static Role GetRole(int Id)
    {
        throw new NotImplementedException();
    }

    public static List<Role> ListRoles()
    {
        throw new NotImplementedException();
    }
}
