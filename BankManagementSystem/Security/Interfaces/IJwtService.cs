using BankManagementSystem.Modules.Users.Models;

namespace BankManagementSystem.Security.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(User user);
    }
}