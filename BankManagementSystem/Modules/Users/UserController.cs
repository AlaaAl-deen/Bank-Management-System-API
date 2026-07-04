using Microsoft.AspNetCore.Mvc;

namespace BankManagementSystem.Modules.Users
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
