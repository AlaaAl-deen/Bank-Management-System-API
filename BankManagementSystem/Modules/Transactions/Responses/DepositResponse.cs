using BankManagementSystem.Common.Responses;

namespace BankManagementSystem.Modules.Transactions.Responses
{
    public class DepositResponse : BaseResponse
    {
        public decimal NewBalance { get; set; }
    }
}
