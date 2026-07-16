namespace BankManagementSystem.Modules.Transactions.Models
{
    public class Transaction
    {
        public long ReferenceNumber { get; set; }

        public int TransactionTypeId { get; set; }

        public int TransactionStatusId { get; set; }

        public int? FromAccountId { get; set; }

        public int? ToAccountId { get; set; }

        public decimal Amount { get; set; }

        public decimal ExchangeRate { get; set; }

        public string Description { get; set; }

        public int CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
