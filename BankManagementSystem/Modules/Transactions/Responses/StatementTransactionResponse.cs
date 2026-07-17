namespace BankManagementSystem.Modules.Transactions.Responses
{
    public class StatementTransactionResponse
    {
        public long ReferenceNumber { get; set; }

        public string TransactionType { get; set; }

        public decimal Amount { get; set; }

        public decimal ExchangeRate { get; set; }

        public string Description { get; set; }

        public DateTime CreatedAt { get; set; }
        public string TransactionDirection { get; set; }
    }
}
