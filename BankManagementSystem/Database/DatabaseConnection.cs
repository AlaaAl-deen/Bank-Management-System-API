using System.Data.SqlClient;
namespace BankManagementSystem.Database
{
    public class DatabaseConnection
    {
        private readonly string _connectionString =
          "Server=db60364.databaseasp.net; Database=db60364; User Id=db60364; Password=eZ_8?2Qk3Lw#; Encrypt=False; MultipleActiveResultSets=True;";

        // إنشاء وإرجاع اتصال جديد بقاعدة البيانات
        public SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
