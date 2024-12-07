using Oracle.ManagedDataAccess.Client;
using WebApp.Util;

namespace WebApp.Models;

public class Location
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required TimeOnly AvailabilityStart  { get; set; }
    public required TimeOnly AvailabilityEnd { get; set; }
    public City City { get; set; }

    public void Persist()
    {
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            OracleCommand cmd = conn.CreateCommand();
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = "edit_location";
            cmd.Parameters.Add(Id);
            cmd.Parameters.Add(Name);
            cmd.Parameters.Add(AvailabilityStart);
            cmd.Parameters.Add(AvailabilityEnd);
            cmd.Parameters.Add(City.Id);
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
            cmd.CommandText = "SELECT ID_LOCATION, name, availability_start, availability_end, id_city FROM LOCATIONS_V WHERE ID_LOCATION = :1";
            cmd.Parameters.Add(Id);
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
            cmd.CommandText = "SELECT ID_LOCATION, name, availability_start, availability_end, id_city FROM LOCATIONS_V";
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
}
