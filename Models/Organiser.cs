using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
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

    public void Persist()
    {
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            OracleCommand cmd = conn.CreateCommand();
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = "reservations_pkg.edit_organizer";
            cmd.Parameters.Add("id_organizer", Id);
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

    public static Organiser GetOrganiser(int Id)
    {
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            OracleCommand cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT id_organizer, name, email, id_role FROM ORGANIZERS_V WHERE id_organizer = :id";
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
                    Role = Role.GetRole(reader.GetInt32(3))
                };
                reader.Close();
                conn.Close();
                return org;
            }
        }
    }

    public static Organiser FindOrganiser(string Email)
    {
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            OracleCommand cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT id_organizer, name, email, id_role FROM ORGANIZERS_V WHERE email = :email";
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
            cmd.CommandText = "SELECT id_organizer, name, email, id_role FROM ORGANIZERS_V";
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
