using BankManagementSystem.Database;
using BankManagementSystem.Modules.Users.Responses;

namespace BankManagementSystem.Modules.Users.Services
{
    public class UserService
    {
        private readonly DatabaseConnection _database;

        public UserService()
        {
            _database = new DatabaseConnection();
        }

       
    }
}
