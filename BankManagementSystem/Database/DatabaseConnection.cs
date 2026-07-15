using Microsoft.Data.SqlClient;
namespace BankManagementSystem.Database
{
    public class DatabaseConnection
    {
        private readonly string _connectionString =
          "Server=ALAA;Database=BankManagementSystemDB;Integrated Security=True;TrustServerCertificate=True;";

        // إنشاء وإرجاع اتصال جديد بقاعدة البيانات
        public SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
