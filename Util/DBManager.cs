using Oracle.ManagedDataAccess.Client;

namespace WebApp.Util
{
    public class DBManager
    {
        public static OracleConnection GetConnection()
        {
            return new OracleConnection(System.Environment.GetEnvironmentVariable("DB_STRING"));
        }
    }
}
