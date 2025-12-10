using Oracle.ManagedDataAccess.Client;
using System.ComponentModel.DataAnnotations;
using WebApp.Util;

namespace WebApp.Models;

public class Location
{
    public int Id { get; set; }
    
    public string Name { get; set; }
   
    [Required]
    [Display(Name = "Availability Start")]
    public TimeOnly AvailabilityStart  { get; set; }
   
    [Required]
    [Display(Name = "Availability End")]
    public TimeOnly AvailabilityEnd { get; set; }

    public City? City { get; set; }

    public Organiser Organiser { get; set; }

    public void Persist()
    {
        if (City == null)
        {
            throw new InvalidOperationException("City is required for location.");
        }
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            OracleCommand cmd = conn.CreateCommand();
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = "locations_pkg.persist_location";
            cmd.Parameters.Add("location_id", Id == 0 ? (object)DBNull.Value : Id);
            cmd.Parameters.Add("name", Name);
            cmd.Parameters.Add("availability_start", new DateTime(2000,1,1) + AvailabilityStart.ToTimeSpan());
            cmd.Parameters.Add("availability_end", new DateTime(2000, 1, 1) + AvailabilityEnd.ToTimeSpan());
            cmd.Parameters.Add("id_city", City.Id);
            cmd.Parameters.Add("id_organiser", 1); 
            int rows = cmd.ExecuteNonQuery();
            if (rows == 0)
            {
                throw new ApplicationException("Persisting entity failed, no rows were updated.");
            }
        }
    }

    public static Location GetLocation(int Id)
    {
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();    
            OracleCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT ID_LOCATION, ""name"", availability_start, availability_end, id_city FROM LOCATIONS_V WHERE ID_LOCATION = :id";
            cmd.Parameters.Add("id", Id);
            cmd.CommandType = System.Data.CommandType.Text;
            using (OracleDataReader reader = cmd.ExecuteReader())
            {
                if (!reader.Read())
                {
                    throw new KeyNotFoundException();
                }
                Location location = new Location
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    AvailabilityStart = TimeOnly.FromDateTime(reader.GetDateTime(2)),
                    AvailabilityEnd = TimeOnly.FromDateTime(reader.GetDateTime(3)),
                    City = City.GetCity(reader.GetInt32(4)),
                };
                reader.Close();
                conn.Close();
                return location;
            }
        }
    }

    public static List<Location> ListLocations()
    {
        List<Location> list = new List<Location>();
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            OracleCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT ID_LOCATION, ""name"", availability_start, availability_end, id_city FROM LOCATIONS_V";
            cmd.CommandType = System.Data.CommandType.Text;
            using (OracleDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    list.Add(new Location
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        AvailabilityStart = TimeOnly.FromDateTime(reader.GetDateTime(2)),
                        AvailabilityEnd = TimeOnly.FromDateTime(reader.GetDateTime(3)),
                        City = City.GetCity(reader.GetInt32(4)),
                    });
                }
            }
        }
        return list;
    }


    public void Delete()
    {
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            OracleCommand cmd = conn.CreateCommand();
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = "locations_pkg.delete_location";
            cmd.Parameters.Add("p_id_location", Id);

            cmd.ExecuteNonQuery();
        }
    }
}
