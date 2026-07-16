using BankManagementSystem.Common.Responses;

namespace BankManagementSystem.Modules.Accounts.Responses
{
    public class AddCurrencyAccountResponse : BaseResponse
    {
        public long AccountNumber { get; set; }

        public string CurrencyCode { get; set; } = string.Empty;
    }
}
