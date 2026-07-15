namespace BankManagementSystem.Modules.Authentication.Requests
{
    public class LoginRequest
    {
        public int CustomerNumber { get; set; }

        public string Password { get; set; }
    }
}
