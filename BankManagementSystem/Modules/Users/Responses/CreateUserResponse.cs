using BankManagementSystem.Common.Responses;

namespace BankManagementSystem.Modules.Users.Responses
{
    public class CreateUserResponse : BaseResponse
    {
        
        public int UserId { get; set; }

        public int CustomerNumber { get; set; }

        public long AccountNumber { get; set; }
    }
}
