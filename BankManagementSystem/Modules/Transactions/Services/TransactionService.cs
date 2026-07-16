using BankManagementSystem.Modules.Transactions.Requests;

namespace BankManagementSystem.Modules.Transactions.Services
{
    public class TransactionService
    {
        private void ValidateDepositRequest(DepositRequest request)
        {
            if (request == null)
            {
                throw new Exception("Request cannot be null.");
            }

            if (request.AccountNumber <= 0)
            {
                throw new Exception("Account number is required.");
            }

            if (request.Amount <= 0)
            {
                throw new Exception("Amount must be greater than zero.");
            }
        }


    }
}
