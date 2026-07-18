using BankManagementSystem.Common.Responses;

namespace BankManagementSystem.Modules.Users.Responses
{
    public class GetAllCustomersResponse : BaseResponse
    {
        public List<CustomerResponse> Customers { get; set; } = new();
    }
}
