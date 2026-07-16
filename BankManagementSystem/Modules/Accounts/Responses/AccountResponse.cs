namespace BankManagementSystem.Modules.Accounts.Responses
{
    public class AccountResponse
    {
        public long AccountNumber { get; set; }

        public string CurrencyCode { get; set; } = string.Empty;

        public decimal Balance { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}
