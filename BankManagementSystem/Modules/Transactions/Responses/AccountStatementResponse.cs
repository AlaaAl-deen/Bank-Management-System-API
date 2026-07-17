using BankManagementSystem.Common.Responses;

namespace BankManagementSystem.Modules.Transactions.Responses
{
    public class AccountStatementResponse : BaseResponse
    {
        public long AccountNumber { get; set; }

        public int CustomerNumber { get; set; }

        public string CustomerName { get; set; }

        public string Currency { get; set; }

        public decimal CurrentBalance { get; set; }

        public List<StatementTransactionResponse> Transactions { get; set; }
    }
}
