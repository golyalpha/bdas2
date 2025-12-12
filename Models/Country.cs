using Oracle.ManagedDataAccess.Client;
using System.Data;
using WebApp.Util;

namespace WebApp.Models
{
    public class Country
    {
        public int Id { get; set; }
        public required string Name { get; set; }

        public void Persist()
        {
            using (OracleConnection conn = DBManager.GetConnection())
            {
                conn.Open();
                OracleCommand cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "edit_country";
                cmd.Parameters.Add("id_country", Id);
                cmd.Parameters.Add("name", Name);
                int rows = cmd.ExecuteNonQuery();
                if (rows == 0)
                {
                    throw new ApplicationException("Persisting entity failed, no rows were updated.");
                }
            }
        }

        public static Country GetCountry(int Id)
        {
            using (OracleConnection conn = DBManager.GetConnection())
            {
                conn.Open();
                OracleCommand cmd = conn.CreateCommand();
                cmd.CommandText = @"SELECT id_country, ""name"" FROM COUNTRIES_V WHERE id_country = :id";
                cmd.Parameters.Add("id", Id);
                cmd.CommandType = System.Data.CommandType.Text;
                using (OracleDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        throw new KeyNotFoundException();
                    }
                    Country country = new Country
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1)
                    };
                    reader.Close();
                    conn.Close();
                    return country;
                }
            }
        }

        public static List<Country> ListCountries()
        {
            List<Country> list = new();
            using (OracleConnection conn = DBManager.GetConnection())
            {
                conn.Open();
                OracleCommand cmd = conn.CreateCommand();
                cmd.CommandText = @"SELECT id_country, ""name"" FROM COUNTRIES_V";
                cmd.CommandType = System.Data.CommandType.Text;
                using (OracleDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Country
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1)
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
                cmd.CommandText = "countries_pkg.delete_country";
                cmd.Parameters.Add("p_id_country", Id);

                cmd.ExecuteNonQuery();
            }
        }
    }
}

