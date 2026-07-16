namespace BankManagementSystem.Modules.Authentication.Requests
{
    public class ChangePasswordRequest
    {
        public int CustomerNumber { get; set; }

        public string CurrentPassword { get; set; } = string.Empty;

        public string NewPassword { get; set; } = string.Empty;
    }
}
