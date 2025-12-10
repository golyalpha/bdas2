using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using WebApp.Util;

namespace WebApp.Models;

public class Credential
{
    public int? Id { get; set; }
    public required string CredentialType { get; set; }
    public required string Data { get; set; }
    public required int IdOrganizer { get; set; }

    /// <summary>
    /// Hashuje heslo pomocí PL/SQL funkce hash_password
    /// </summary>
    public static string HashPassword(string password)
    {
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            using var cmd = conn.CreateCommand();
            // OPRAVA: Funkce se volá pøes SELECT, ne jako stored procedura
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.CommandText = "SELECT user_management_pkg.hash_password(:p_password) FROM DUAL";
            cmd.Parameters.Add("p_password", OracleDbType.Varchar2).Value = password;
            
            object result = cmd.ExecuteScalar();
            string hash = result?.ToString() ?? throw new Exception("Hash failed");
            return hash;
        }
    }

    /// <summary>
    /// Ovìøí heslo pomocí PL/SQL funkce verify_password
    /// Vrací ID organizátora nebo null pøi neúspìchu
    /// </summary>
    public static int? VerifyPassword(string email, string password)
    {
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            using var cmd = conn.CreateCommand();
            // OPRAVA: Funkce se volá pøes SELECT
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.CommandText = "SELECT user_management_pkg.verify_password(:p_email, :p_password) FROM DUAL";
            cmd.Parameters.Add("p_email", OracleDbType.Varchar2).Value = email;
            cmd.Parameters.Add("p_password", OracleDbType.Varchar2).Value = password;
            
            object result = cmd.ExecuteScalar();
            
            if (result == null || result == DBNull.Value || result is OracleDecimal dec && dec.IsNull)
            {
                return null;
            }
            
            return Convert.ToInt32(result.ToString());
        }
    }

    public void Persist()
    {
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            OracleCommand cmd = conn.CreateCommand();
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = "user_management_pkg.persist_credential";
            
            // OPRAVA: Správné parametry podle package specifikace
            cmd.Parameters.Add("p_id_credential", OracleDbType.Int32).Value = Id ?? (object)DBNull.Value;
            cmd.Parameters.Add("p_credential_type", OracleDbType.Char, 8).Value = CredentialType;
            cmd.Parameters.Add("p_data", OracleDbType.Varchar2, 256).Value = Data;
            cmd.Parameters.Add("p_id_organizer", OracleDbType.Int32).Value = IdOrganizer;
            
            int rows = cmd.ExecuteNonQuery();
            if (rows == 0)
            {
                throw new ApplicationException("Persisting entity failed, no rows were updated.");
            }
        }
    }
}
