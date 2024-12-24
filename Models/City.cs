using Oracle.ManagedDataAccess.Client;
using System.Data;
using WebApp.Util;

namespace WebApp.Models
{
    public class City
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Country Country { get; set; }



        public void Persist()
        {
            using (OracleConnection conn = DBManager.GetConnection())
            {
                conn.Open();
                OracleCommand cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "edit_city";
                cmd.Parameters.Add("id_city", Id);
                cmd.Parameters.Add("name", Name);
                cmd.Parameters.Add("id_country", Country.Id);
                int rows = cmd.ExecuteNonQuery();
                if (rows == 0)
                {
                    throw new ApplicationException("Persisting entity failed, no rows were updated.");
                }
            }
        }

        public static City GetCity(int Id)
        {
            using (OracleConnection conn = DBManager.GetConnection())
            {
                conn.Open();
                OracleCommand cmd = conn.CreateCommand();
                cmd.CommandText = @"SELECT id_city, ""name"", id_country FROM CITIES_V WHERE id_city = :id";
                cmd.Parameters.Add("id", Id);
                cmd.CommandType = System.Data.CommandType.Text;
                using (OracleDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        throw new KeyNotFoundException();
                    }
                    City city = new City
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Country = Country.GetCountry(reader.GetInt32(2))
                    };
                    reader.Close();
                    conn.Close();
                    return city;
                }
            }
        }

        public static List<City> ListCities()
        {
            List<City> list = new();
            using (OracleConnection conn = DBManager.GetConnection())
            {
                conn.Open();
                OracleCommand cmd = conn.CreateCommand();
                cmd.CommandText = @"SELECT id_city, ""name"", id_country FROM CITIES_V";
                cmd.CommandType = System.Data.CommandType.Text;
                using (OracleDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new City
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Country = Country.GetCountry(reader.GetInt32(2))
                        });
                    }
                }
            }
            return list;
        }
    }
}
