using Microsoft.AspNetCore.Authorization.Infrastructure;
using Oracle.ManagedDataAccess.Client;
using WebApp.Util;

namespace WebApp.Models;

public class Organiser
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }

    public static Organiser GetOrganiser(int Id)
    {
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            OracleCommand cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT id_organizer, name, email FROM ORGANIZERS_V WHERE id_organizer = :1";
            cmd.Parameters.Add(Id);
            cmd.CommandType = System.Data.CommandType.Text;
            using (OracleDataReader reader = cmd.ExecuteReader())
            {
                if (!reader.Read())
                {
                    throw new KeyNotFoundException();
                }
                Organiser org = new Organiser
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Email = reader.GetString(2),
                };
                reader.Close();
                conn.Close();
                return org;
            }
        }
    }

    public static List<Organiser> GetOrganisers()
    {
        List<Organiser> list = new List<Organiser>();
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            OracleCommand cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT id_organizer, name, email FROM CREDENTIALS_V";
            cmd.CommandType = System.Data.CommandType.Text;
            using (OracleDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    list.Add(new Organiser
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Email = reader.GetString(2),
                    });
                }
            }
        }
        return list;
    }
}
