using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.ComponentModel.DataAnnotations;
using WebApp.Util;

namespace WebApp.Models;

public class Organiser
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; }

    [Required]
    public string Email { get; set; }

    [Required]
    public Role Role { get; set; }

    // Náhradník (substitute)
    public Organiser? Substitute { get; set; }

    public void Persist()
    {
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            OracleCommand cmd = conn.CreateCommand();
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = "reservations_pkg.edit_organizer";
            cmd.Parameters.Add("id_organizer", Id == 0 ? (object)DBNull.Value : Id);
            cmd.Parameters.Add("name", Name);
            cmd.Parameters.Add("email", Email);
            cmd.Parameters.Add("id_role", Role.Id);
            int rows = cmd.ExecuteNonQuery();
            if (rows == 0)
            {
                throw new ApplicationException("Persisting entity failed, no rows were updated.");
            }
        }
    }

    /// <summary>
    /// Aktualizuje pouze náhradníka organizátora (obchází trigger pro non-transferable FK)
    /// </summary>
    public void UpdateSubstitute(int? substituteId)
    {
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            OracleCommand cmd = conn.CreateCommand();
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = "reservations_pkg.update_organizer_substitute";
            cmd.Parameters.Add("id_organizer", Id);
            cmd.Parameters.Add("id_organizer_substitute", substituteId.HasValue ? (object)substituteId.Value : DBNull.Value);
            cmd.ExecuteNonQuery();
        }
    }

    public static Organiser GetOrganiser(int Id)
    {
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            OracleCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT id_organizer, ""name"", email, id_role, id_organizer_substitute 
                               FROM ORGANIZERS_V WHERE id_organizer = :id";
            cmd.Parameters.Add("id", Id);
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
                    Role = Role.GetRole(reader.GetInt32(3)),
                    Substitute = reader.IsDBNull(4) ? null : GetOrganiserBasic(reader.GetInt32(4))
                };
                reader.Close();
                conn.Close();
                return org;
            }
        }
    }

    /// <summary>
    /// Získá základní info o organizátorovi bez rekurzivního načítání náhradníka
    /// </summary>
    private static Organiser GetOrganiserBasic(int Id)
    {
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            OracleCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT id_organizer, ""name"", email, id_role 
                               FROM ORGANIZERS_V WHERE id_organizer = :id";
            cmd.Parameters.Add("id", Id);
            cmd.CommandType = System.Data.CommandType.Text;
            using (OracleDataReader reader = cmd.ExecuteReader())
            {
                if (!reader.Read())
                {
                    return null;
                }
                return new Organiser
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Email = reader.GetString(2),
                    Role = Role.GetRole(reader.GetInt32(3))
                };
            }
        }
    }

    public static Organiser FindOrganiser(string Email)
    {
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            OracleCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT id_organizer, ""name"", email, id_role FROM ORGANIZERS_V WHERE email = :email";
            cmd.Parameters.Add("email", Email);
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
            cmd.CommandText = @"SELECT id_organizer, ""name"", email, id_role, id_organizer_substitute FROM ORGANIZERS_V";
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
                        Role = Role.GetRole(reader.GetInt32(3)),
                        Substitute = reader.IsDBNull(4) ? null : new Organiser { Id = reader.GetInt32(4) }
                    });
                }
            }
        }
        
        // Doplnění jmen náhradníků
        var orgDict = list.ToDictionary(o => o.Id);
        foreach (var org in list)
        {
            if (org.Substitute != null && orgDict.TryGetValue(org.Substitute.Id, out var sub))
            {
                org.Substitute = new Organiser 
                { 
                    Id = sub.Id, 
                    Name = sub.Name, 
                    Email = sub.Email,
                    Role = sub.Role
                };
            }
        }
        
        return list;
    }

    /// <summary>
    /// Vrátí seznam organizátorů, kteří mohou být náhradníky pro daného uživatele
    /// (všichni kromě sebe sama)
    /// </summary>
    public static List<Organiser> GetPotentialSubstitutes(int excludeOrganiserId)
    {
        return ListOrganisers().Where(o => o.Id != excludeOrganiserId).ToList();
    }

    public static List<Organiser> GetNonAdminOrganisers()
    {
        List<Organiser> list = new List<Organiser>();
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            OracleCommand cmd = conn.CreateCommand();
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = "user_management_pkg.get_non_admin_organizers";
            
            OracleParameter cursorParam = new OracleParameter();
            cursorParam.ParameterName = "p_cursor";
            cursorParam.OracleDbType = OracleDbType.RefCursor;
            cursorParam.Direction = System.Data.ParameterDirection.Output;
            cmd.Parameters.Add(cursorParam);

            cmd.ExecuteNonQuery();

            using (OracleDataReader reader = ((OracleRefCursor)cursorParam.Value).GetDataReader())
            {
                while (reader.Read())
                {
                    list.Add(new Organiser
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Email = reader.GetString(2),
                        Role = new Role
                        {
                            Id = reader.GetInt32(3),
                            Name = reader.GetString(4)
                        }
                    });
                }
            }
        }
        return list;
    }
}
