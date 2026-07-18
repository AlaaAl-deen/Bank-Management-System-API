using BankManagementSystem.Common.Responses;

namespace BankManagementSystem.Modules.Authentication.Responses
{
    public class LoginResponse : BaseResponse
    {
        public int UserId { get; set; }

        public int CustomerNumber { get; set; }

        public int RoleId { get; set; }

        //we added becuse JWT
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        //public bool MustChangePassword { get; set; }

        
        public string Token { get; set; } = string.Empty;
    }
}
