using BankManagementSystem.Common.Responses;

namespace BankManagementSystem.Modules.Accounts.Responses
{
    public class GetAccountsResponse : BaseResponse
    {
        public List<AccountResponse> Accounts { get; set; } = new();
    }
}
