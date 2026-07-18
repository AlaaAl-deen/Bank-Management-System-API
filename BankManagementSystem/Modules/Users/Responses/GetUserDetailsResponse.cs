using BankManagementSystem.Common.Responses;

namespace BankManagementSystem.Modules.Users.Responses
{
    public class GetUserDetailsResponse : BaseResponse
    {
        public int CustomerNumber { get; set; }

        public string FirstName { get; set; }

        public string SecondName { get; set; }

        public string ThirdName { get; set; }

        public string LastName { get; set; }

        public string FullName { get; set; }

        public string PhoneNumber { get; set; }

        public string Address { get; set; }

        public string Role { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
