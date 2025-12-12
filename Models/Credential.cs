using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.ComponentModel.DataAnnotations;
using WebApp.Util;

namespace WebApp.Models;

public class Credential
{
    public int Id { get; set; }

    [Required]
    public string CredentialType { get; set; }

    [Required]
    public string Data { get; set; }

    public DateTime CreatedAt { get; set; }

    public int IdOrganizer { get; set; }

    public void Persist()
    {
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            OracleCommand cmd = conn.CreateCommand();
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = "user_management_pkg.persist_credential";
            cmd.Parameters.Add("p_id_credential", Id == 0 ? (object)DBNull.Value : Id);
            cmd.Parameters.Add("p_credential_type", CredentialType);
            cmd.Parameters.Add("p_data", Data);
            cmd.Parameters.Add("p_id_organizer", IdOrganizer);
            cmd.ExecuteNonQuery();
        }
    }

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

    /// <summary>
    /// Zmìní heslo uživatele
    /// </summary>
    /// <param name="userId">ID uživatele</param>
    /// <param name="oldPassword">Staré heslo (plain text)</param>
    /// <param name="newPassword">Nové heslo (plain text)</param>
    /// <exception cref="Exception">Pokud staré heslo není správné nebo jiná chyba</exception>
    public static void ChangePassword(int userId, string oldPassword, string newPassword)
    {
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = "user_management_pkg.change_password";
            cmd.Parameters.Add("p_id_organizer", userId);
            cmd.Parameters.Add("p_old_password", oldPassword);
            cmd.Parameters.Add("p_new_password", newPassword);
            
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Oracle.ManagedDataAccess.Client.OracleException ex)
            {
                // ORA-20101: Staré heslo je nesprávné
                if (ex.Number == 20101)
                {
                    throw new Exception("Staré heslo je nesprávné");
                }
                // ORA-20102: Nové heslo musí být odlišné
                else if (ex.Number == 20102)
                {
                    throw new Exception("Nové heslo musí být odlišné od starého");
                }
                // ORA-20103: Uživatel nebyl nalezen
                else if (ex.Number == 20103)
                {
                    throw new Exception("Uživatel nebyl nalezen");
                }
                else
                {
                    throw new Exception($"Chyba pøi zmìnì hesla: {ex.Message}");
                }
            }
        }
    }

    /// <summary>
    /// Resetuje heslo uživatele podle emailu (pro zapomenuté heslo)
    /// </summary>
public static void ResetPassword(string email, string newPassword)
    {
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = "user_management_pkg.reset_password";
            cmd.Parameters.Add("p_email", email);
            cmd.Parameters.Add("p_new_password", newPassword);
            
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Oracle.ManagedDataAccess.Client.OracleException ex)
            {
                // ORA-20104: Uživatel nebyl nalezen
                if (ex.Number == 20104)
                {
                    throw new Exception("Uživatel s tímto emailem nebyl nalezen");
                }
                else
                {
                    throw new Exception($"Chyba pøi resetování hesla: {ex.Message}");
                }
            }
        }
    }
}
