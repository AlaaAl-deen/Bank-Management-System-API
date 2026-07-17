namespace BankManagementSystem.Modules.Transactions.Requests
{
    public class TransferRequest
    {
        public long FromAccountNumber { get; set; }

        public long ToAccountNumber { get; set; }

        public decimal Amount { get; set; }
    }
}
