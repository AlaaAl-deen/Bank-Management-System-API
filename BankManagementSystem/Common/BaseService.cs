using BankManagementSystem.Database;
using Microsoft.Data.SqlClient;

namespace BankManagementSystem.Common
{
    public abstract class BaseService
    {
        protected readonly DatabaseConnection _database;

        protected BaseService()
        {
            _database = new DatabaseConnection();
        }

        protected SqlConnection GetConnection()
        {
            return _database.GetConnection();
        }
    }
}
