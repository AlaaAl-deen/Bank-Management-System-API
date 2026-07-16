namespace BankManagementSystem.Modules.Transactions.Models
{
    public class Account
    {
        public int AccountId { get; set; }

        public long AccountNumber { get; set; }

        public int UserId { get; set; }

        public int CurrencyId { get; set; }

        public decimal Balance { get; set; }

        public int AccountStatusId { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
