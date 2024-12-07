using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using System.ComponentModel.DataAnnotations;
using System.Data;
using WebApp.Util;

namespace WebApp.Models;

public class Role
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; }

    public void Persist()
    {
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            OracleCommand cmd = conn.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "edit_role";
            cmd.Parameters.Add(Id);
            cmd.Parameters.Add(Name);
            int rows = cmd.ExecuteNonQuery();
            if (rows == 0)
            {
                throw new ApplicationException("Persisting entity failed, no rows were updated.");
            }
        }
    }

    public static Role GetRole(int Id)
    {
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            OracleCommand cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT id_role, name FROM ROLES_V WHERE id_role = :1";
            cmd.Parameters.Add(Id);
            cmd.CommandType = System.Data.CommandType.Text;
            using (OracleDataReader reader = cmd.ExecuteReader())
            {
                if (!reader.Read())
                {
                    throw new KeyNotFoundException();
                }
                Role role = new Role
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1)
                };
                reader.Close();
                conn.Close();
                return role;
            }
        }
    }

    public static List<Role> ListRoles()
    {
        List<Role> list = new();
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            OracleCommand cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT id_role, name FROM ROLES_V";
            cmd.CommandType = System.Data.CommandType.Text;
            using (OracleDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    list.Add(new Role
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1)
                    });
                }
            }
        }
        return list;
    }
}
