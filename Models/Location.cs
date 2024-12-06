using Oracle.ManagedDataAccess.Client;
using WebApp.Util;

namespace WebApp.Models;

public class Location
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required TimeOnly AvailabilityStart  { get; set; }
    public required TimeOnly AvailabilityEnd { get; set; }
    private int CityId;
    private int CountryId;
    public string City {
        get {
            using (OracleConnection conn = DBManager.GetConnection())
            {
                conn.Open();
                OracleCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT name FROM CITIES_V WHERE ID_CITY = :1";
                cmd.Parameters.Add(CityId);
                cmd.CommandType = System.Data.CommandType.Text;
                using (OracleDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        throw new KeyNotFoundException();
                    }
                    return reader.GetString(0);
                }
            }
        }
        set {
            using (OracleConnection conn = DBManager.GetConnection())
            {
                conn.Open();
                OracleCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT ID_CITY FROM CITIES_V WHERE name = :1";
                cmd.Parameters.Add(value);
                cmd.CommandType = System.Data.CommandType.Text;
                using (OracleDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        throw new KeyNotFoundException();
                    }
                    CityId = reader.GetInt32(0);
                }
            }
        }
    }
    public string Country {
        get
        {
            using (OracleConnection conn = DBManager.GetConnection())
            {
                conn.Open();
                OracleCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT name FROM COUNTRIES_V WHERE ID_COUNTRY = :1";
                cmd.Parameters.Add(CountryId);
                cmd.CommandType = System.Data.CommandType.Text;
                using (OracleDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        throw new KeyNotFoundException();
                    }
                    return reader.GetString(0);
                }
            }
        }
        set
        {
            using (OracleConnection conn = DBManager.GetConnection())
            {
                conn.Open();
                OracleCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT ID_COUNTRY FROM COUNTRIES_V WHERE name = :1";
                cmd.Parameters.Add(value);
                cmd.CommandType = System.Data.CommandType.Text;
                using (OracleDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        throw new KeyNotFoundException();
                    }
                    CountryId = reader.GetInt32(0);
                }
            }
        }
    }

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
            cmd.Parameters.Add(City);
            cmd.Parameters.Add(Country);
            int rows = cmd.ExecuteNonQuery();
            if (rows == 0)
            {
                throw new ApplicationException("Persisting entity failed, no rows were updated.");
            }
        }
    }

    public static List<string> GetCityList() {
        List<string> list = new();
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            OracleCommand country_cmd = conn.CreateCommand();
            country_cmd.CommandText = "SELECT ID_COUNTRY, name FROM COUNTRIES_V";
            country_cmd.CommandType = System.Data.CommandType.Text;
            using (OracleDataReader country_reader = country_cmd.ExecuteReader())
            {
                while (country_reader.Read())
                {
                    OracleCommand city_cmd = conn.CreateCommand();
                    city_cmd.CommandText = "SELECT ID_CITY, name FROM CITIES_V WHERE ID_COUNTRY = :1";
                    city_cmd.CommandType = System.Data.CommandType.Text;
                    city_cmd.Parameters.Add(country_reader.GetInt32(0));
                    using (OracleDataReader city_reader = city_cmd.ExecuteReader())
                    {
                        while (city_reader.Read())
                        {
                            list.Add(city_reader.GetString(1) + ";" + country_reader.GetString(1));
                        }
                    }
                }
            }
        }
        return list;
    }

    public static Location GetLocation(int Id)
    {
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            OracleCommand cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT ID_LOCATION, name, availabilityStart, availabilityEnd FROM LOCATIONS_V WHERE ID_LOCATION = :1";
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
                    CityId = reader.GetInt32(4),
                    CountryId = reader.GetInt32(5),
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
            cmd.CommandText = "SELECT ID_LOCATION, name, availability_start, availability_end FROM LOCATIONS_V ";
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
                    });
                }
            }
        }
        return list;
    }
}
