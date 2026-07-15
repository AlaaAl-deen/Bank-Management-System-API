using BankManagementSystem.Common.Responses;

namespace BankManagementSystem.Modules.Authentication.Responses
{
    public class LoginResponse : BaseResponse
    {
        public int UserId { get; set; }

        public int CustomerNumber { get; set; }

        public int RoleId { get; set; }

        public bool MustChangePassword { get; set; }
    }
}
