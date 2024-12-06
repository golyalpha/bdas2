using Oracle.ManagedDataAccess.Client;
using WebApp.Util;

namespace WebApp.Models;

public class Organiser
{
    public int? Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }

    public required Role Role { get; set; }

    public void Persist()
    {
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            OracleCommand cmd = conn.CreateCommand();
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = "edit_organizer";
            cmd.Parameters.Add(Id);
            cmd.Parameters.Add(Name);
            cmd.Parameters.Add(Email);
            cmd.Parameters.Add(Role.Id);
            int rows = cmd.ExecuteNonQuery();
            if (rows == 0)
            {
                throw new ApplicationException("Persisting entity failed, no rows were updated.");
            }
        }
    }

    public static Organiser GetOrganiser(int Id)
    {
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            OracleCommand cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT id_organizer, name, email, role_id FROM ORGANIZERS_V WHERE id_organizer = :1";
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
                    Role = Role.GetRole(reader.GetInt32(3))
                };
                reader.Close();
                conn.Close();
                return org;
            }
        }
    }

    public static List<Organiser> ListOrganisers()
    {
        List<Organiser> list = new List<Organiser>();
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            OracleCommand cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT id_organizer, name, email FROM ORGANIZERS_V";
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
                        Role = Role.GetRole(reader.GetInt32(3))
                    });
                }
            }
        }
        return list;
    }
}
