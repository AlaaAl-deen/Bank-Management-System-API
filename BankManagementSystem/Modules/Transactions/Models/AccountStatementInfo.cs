namespace BankManagementSystem.Modules.Transactions.Models
{
    public class AccountStatementInfo
    {
        public int AccountId { get; set; }

        public long AccountNumber { get; set; }

        public int CustomerNumber { get; set; }

        public string CustomerName { get; set; }

        public string Currency { get; set; }

        public decimal Balance { get; set; }
    }
}
