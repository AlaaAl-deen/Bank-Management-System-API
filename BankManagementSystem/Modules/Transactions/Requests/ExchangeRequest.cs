namespace BankManagementSystem.Modules.Transactions.Requests
{
    public class ExchangeRequest
    {
        public long FromAccountNumber { get; set; }

        public long ToAccountNumber { get; set; }

        public decimal Amount { get; set; }
    }
}
