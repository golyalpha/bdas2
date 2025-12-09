namespace WebApp.Models;
using Oracle.ManagedDataAccess.Client; // ODP.NET Oracle managed provider
using Oracle.ManagedDataAccess.Types;
using System.Xml.Linq;
using WebApp.Util;

public class Credential
{
    public int? Id { get; set; }
    public required string CredentialType { get; set; }
    public required string Data { get; set; }
    public required int IdOrganizer { get; set; }

    public void Persist()
    {
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            OracleCommand cmd = conn.CreateCommand();
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = "user_management_pkg.persist_credential";
            cmd.Parameters.Add("id_credential", Id);
            cmd.Parameters.Add("credential_type", CredentialType);
            cmd.Parameters.Add("data", Data);
            cmd.Parameters.Add("id_organizer", IdOrganizer);
            int rows = cmd.ExecuteNonQuery();
            if (rows == 0)
            {
                throw new ApplicationException("Persisting entity failed, no rows were updated.");
            }
        }
    }

    public static Credential GetCredential(int Id)
    {
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            OracleCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT id_credential, ""data"", credential_type, id_organizer FROM CREDENTIALS_V WHERE id_credential = :1";
            cmd.Parameters.Add(Id);
            cmd.CommandType = System.Data.CommandType.Text;
            using (OracleDataReader reader = cmd.ExecuteReader())
            {
                if (!reader.Read())
                {
                    throw new KeyNotFoundException();
                }
                Credential cred = new Credential
                {
                    Id = reader.GetInt32(0),
                    Data = reader.GetString(1),
                    CredentialType = reader.GetString(2),
                    IdOrganizer = reader.GetInt32(3)
                };
                reader.Close();
                conn.Close();
                return cred;
            }
        }
    }

    public static List<Credential> ListCredentials() {
        List<Credential> list = new List<Credential>();
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            OracleCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT id_credential, ""data"", credential_type, id_organizer FROM CREDENTIALS_V";
            cmd.CommandType = System.Data.CommandType.Text;
            using (OracleDataReader reader = cmd.ExecuteReader())
            { 
                while (reader.Read())
                {
                    list.Add(new Credential
                    {
                        Id = reader.GetInt32(0),
                        Data = reader.GetString(1),
                        CredentialType = reader.GetString(2),
                        IdOrganizer = reader.GetInt32(3)
                    });
                }
            }
        }
        return list;
    }

}
