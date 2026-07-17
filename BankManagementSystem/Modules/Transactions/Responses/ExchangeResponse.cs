using BankManagementSystem.Common.Responses;

namespace BankManagementSystem.Modules.Transactions.Responses
{
    public class ExchangeResponse : BaseResponse
    {
        public decimal SenderNewBalance { get; set; }

        public decimal ReceiverNewBalance { get; set; }

        public decimal ExchangeRate { get; set; }

        public decimal ConvertedAmount { get; set; }

        public long ReferenceNumber { get; set; }
    }
}
