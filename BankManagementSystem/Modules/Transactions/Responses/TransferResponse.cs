using BankManagementSystem.Common.Responses;

namespace BankManagementSystem.Modules.Transactions.Responses
{
    public class TransferResponse : BaseResponse
    {
        public decimal SenderNewBalance { get; set; }

        public decimal ReceiverNewBalance { get; set; }

        public long ReferenceNumber { get; set; }
    }
}
