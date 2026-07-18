using BankManagementSystem.Common.Responses;

namespace BankManagementSystem.Modules.Accounts.Responses
{
    public class GetAccountDetailsResponse : BaseResponse
    {
        public long AccountNumber { get; set; }

        public int CustomerNumber { get; set; }

        public string CustomerName { get; set; }

        public string Currency { get; set; }

        public decimal Balance { get; set; }

        public string Status { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
