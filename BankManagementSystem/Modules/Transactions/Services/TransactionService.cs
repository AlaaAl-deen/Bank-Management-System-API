using BankManagementSystem.Common;
using BankManagementSystem.Modules.Transactions.Models;
using BankManagementSystem.Modules.Transactions.Requests;
using BankManagementSystem.Modules.Transactions.Responses;
using System.Data.SqlClient;

namespace BankManagementSystem.Modules.Transactions.Services
{
    public class TransactionService : BaseService
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

        private Account GetAccountByAccountNumber(
    SqlConnection connection,
    SqlTransaction transaction,
    long accountNumber)
        {
            const string query = @"
    SELECT
        AccountId,
        AccountNumber,
        UserId,
        CurrencyId,
        Balance,
        AccountStatusId,
        CreatedAt,
        UpdatedAt
    FROM Accounts
    WHERE AccountNumber = @AccountNumber";

            using (SqlCommand command = new SqlCommand(query, connection, transaction))
            {
                command.Parameters.AddWithValue("@AccountNumber", accountNumber);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Account
                        {
                            AccountId = Convert.ToInt32(reader["AccountId"]),
                            AccountNumber = Convert.ToInt64(reader["AccountNumber"]),
                            UserId = Convert.ToInt32(reader["UserId"]),
                            CurrencyId = Convert.ToInt32(reader["CurrencyId"]),
                            Balance = Convert.ToDecimal(reader["Balance"]),
                            AccountStatusId = Convert.ToInt32(reader["AccountStatusId"]),
                            CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                            UpdatedAt = reader["UpdatedAt"] == DBNull.Value ? null: Convert.ToDateTime(reader["UpdatedAt"])
                        };
                    }
                }
            }

            return null;
        }

        private bool IsAccountActive(int accountStatusId)
        {
            return accountStatusId == 1;
        }


        private void UpdateBalance(
     SqlConnection connection,
     SqlTransaction transaction,
     int accountId,
     decimal newBalance)
        {
            const string query = @"
    UPDATE Accounts
    SET
        Balance = @Balance,
        UpdatedAt = @UpdatedAt
    WHERE AccountId = @AccountId";

            using (SqlCommand command = new SqlCommand(query, connection, transaction))
            {
                command.Parameters.AddWithValue("@AccountId", accountId);
                command.Parameters.AddWithValue("@Balance", newBalance);
                command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    throw new Exception("Failed to update account balance.");
                }
            }
        }

        private long GenerateReferenceNumber(
     SqlConnection connection,
     SqlTransaction transaction)
        {
            const string query = @"
    SELECT ISNULL(MAX(ReferenceNumber), 5000000000)
    FROM Transactions";

            using (SqlCommand command = new SqlCommand(query, connection, transaction))
            {
                object result = command.ExecuteScalar();

                return Convert.ToInt64(result) + 1;
            }
        }

        private void CreateTransaction(
    SqlConnection connection,
    SqlTransaction transaction,
    Transaction transactionModel)
        {
            const string query = @"
    INSERT INTO Transactions
    (
        ReferenceNumber,
        TransactionTypeId,
        TransactionStatusId,
        FromAccountId,
        ToAccountId,
        Amount,
        ExchangeRate,
        Description,
        CreatedBy,
        CreatedAt
    )
    VALUES
    (
        @ReferenceNumber,
        @TransactionTypeId,
        @TransactionStatusId,
        @FromAccountId,
        @ToAccountId,
        @Amount,
        @ExchangeRate,
        @Description,
        @CreatedBy,
        @CreatedAt
    )";

            using (SqlCommand command = new SqlCommand(query, connection, transaction))
            {
                command.Parameters.AddWithValue("@ReferenceNumber", transactionModel.ReferenceNumber);
                command.Parameters.AddWithValue("@TransactionTypeId", transactionModel.TransactionTypeId);
                command.Parameters.AddWithValue("@TransactionStatusId", transactionModel.TransactionStatusId);
                command.Parameters.AddWithValue("@FromAccountId", (object?)transactionModel.FromAccountId ?? DBNull.Value);
                command.Parameters.AddWithValue("@ToAccountId", (object?)transactionModel.ToAccountId ?? DBNull.Value);
                command.Parameters.AddWithValue("@Amount", transactionModel.Amount);
                command.Parameters.AddWithValue("@ExchangeRate", transactionModel.ExchangeRate);
                command.Parameters.AddWithValue("@Description", transactionModel.Description);
                command.Parameters.AddWithValue("@CreatedBy", transactionModel.CreatedBy);
                command.Parameters.AddWithValue("@CreatedAt", transactionModel.CreatedAt);

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    throw new Exception("Failed to create transaction.");
                }
            }
        }

        public DepositResponse Deposit(DepositRequest request)
        {
            DepositResponse response = new DepositResponse();

            using (SqlConnection connection = GetConnection())
            {
                connection.Open();

                SqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    // Validate Request
                    ValidateDepositRequest(request);

                    // Get Account
                    Account account = GetAccountByAccountNumber(
                        connection,
                        transaction,
                        request.AccountNumber);

                    if (account == null)
                    {
                        throw new Exception("Account not found.");
                    }

                    // Check Account Status
                    if (!IsAccountActive(account.AccountStatusId))
                    {
                        throw new Exception("Account is inactive.");
                    }

                    // Calculate New Balance
                    decimal newBalance = account.Balance + request.Amount;

                    // Update Balance
                    UpdateBalance(
                        connection,
                        transaction,
                        account.AccountId,
                        newBalance);

                    // Generate Reference Number
                    long referenceNumber = GenerateReferenceNumber(
                        connection,
                        transaction);

                    // Create Transaction
                    Transaction transactionModel = new Transaction
                    {
                        ReferenceNumber = referenceNumber,
                        TransactionTypeId = 1,          // Deposit
                        TransactionStatusId = 2,        // Completed
                        FromAccountId = null,
                        ToAccountId = account.AccountId,
                        Amount = request.Amount,
                        ExchangeRate = 1,
                        Description = "Cash Deposit",
                        CreatedBy = 1,     // Admin
                        CreatedAt = DateTime.Now
                    };

                    CreateTransaction(
                        connection,
                        transaction,
                        transactionModel);

                    // Commit Transaction
                    transaction.Commit();

                    response.Success = true;
                    response.Message = "Deposit completed successfully.";
                    response.NewBalance = newBalance;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();

                    response.Success = false;
                    response.Message = ex.Message;
                }
            }

            return response;
        }

    }
}
