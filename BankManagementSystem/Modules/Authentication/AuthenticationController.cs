using Microsoft.AspNetCore.Mvc;

namespace BankManagementSystem.Modules.Authentication
{
    public class AuthenticationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
