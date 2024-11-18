namespace WebApp.Models;
using Oracle.ManagedDataAccess.Client; // ODP.NET Oracle managed provider
using Oracle.ManagedDataAccess.Types;
using WebApp.Util;

public class Credential
{
    public int Id { get; set; }
    public required string CredentialType { get; set; }
    public required string Data { get; set; }

    public static Credential GetCredential(int Id)
    {
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            OracleCommand cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT id_credential, data, credential_type FROM CREDENTIALS_V WHERE id_credential = :1";
            cmd.Parameters.Add(Id);
            cmd.CommandType = System.Data.CommandType.Text;
            using (OracleDataReader reader = cmd.ExecuteReader())
            {
                reader.Read();
                Credential cred = new Credential
                {
                    Id = reader.GetInt32(0),
                    Data = reader.GetString(1),
                    CredentialType = reader.GetString(2),
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
            cmd.CommandText = "SELECT id_credential, data, credential_type FROM CREDENTIALS_V";
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
                    });
                }
            }
        }
        
        return list;
    }

}
