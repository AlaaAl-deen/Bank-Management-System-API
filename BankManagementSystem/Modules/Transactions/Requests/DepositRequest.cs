namespace BankManagementSystem.Modules.Transactions.Requests
{
    public class DepositRequest
    {
        public long AccountNumber { get; set; }

        public decimal Amount { get; set; }
    }
}
