namespace BankManagementSystem.Modules.Accounts.Responses
{
    public class AddCurrencyAccountResponse
    {
        public long AccountNumber { get; set; }

        public string CurrencyCode { get; set; } = string.Empty;
    }
}
