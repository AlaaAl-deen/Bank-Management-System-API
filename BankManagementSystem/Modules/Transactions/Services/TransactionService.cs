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

        //Transfer
        private void ValidateTransferRequest(TransferRequest request)
        {
            if (request == null)
                throw new Exception("Request cannot be null.");

            if (request.FromAccountNumber <= 0)
                throw new Exception("Sender account number is required.");

            if (request.ToAccountNumber <= 0)
                throw new Exception("Receiver account number is required.");

            if (request.FromAccountNumber == request.ToAccountNumber)
                throw new Exception("Sender and receiver accounts cannot be the same.");

            if (request.Amount <= 0)
                throw new Exception("Amount must be greater than zero.");
        }

        private bool HasSufficientBalance(Account account, decimal amount)
        {
            return account.Balance >= amount;
        }


        public TransferResponse Transfer(TransferRequest request)
        {
            TransferResponse response = new TransferResponse();

            using (SqlConnection connection = GetConnection())
            {
                connection.Open();

                SqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    // Validate Request
                    ValidateTransferRequest(request);

                    // Get Sender Account
                    Account senderAccount = GetAccountByAccountNumber(
                        connection,
                        transaction,
                        request.FromAccountNumber);

                    if (senderAccount == null)
                    {
                        throw new Exception("Sender account not found.");
                    }

                    // Get Receiver Account
                    Account receiverAccount = GetAccountByAccountNumber(
                        connection,
                        transaction,
                        request.ToAccountNumber);

                    if (receiverAccount == null)
                    {
                        throw new Exception("Receiver account not found.");
                    }

                    // Check Sender Account Status
                    if (!IsAccountActive(senderAccount.AccountStatusId))
                    {
                        throw new Exception("Sender account is inactive.");
                    }

                    // Check Receiver Account Status
                    if (!IsAccountActive(receiverAccount.AccountStatusId))
                    {
                        throw new Exception("Receiver account is inactive.");
                    }

                    // Check Currency
                    if (senderAccount.CurrencyId != receiverAccount.CurrencyId)
                    {
                        throw new Exception("Transfer is allowed only between accounts with the same currency. Please use the Exchange service.");
                    }
                    // Check Balance
                    if (!HasSufficientBalance(senderAccount, request.Amount))
                    {
                        throw new Exception("Insufficient balance.");
                    }

                    // Calculate New Balances
                    decimal senderNewBalance = senderAccount.Balance - request.Amount;
                    decimal receiverNewBalance = receiverAccount.Balance + request.Amount;

                    // Update Sender Balance
                    UpdateBalance(
                        connection,
                        transaction,
                        senderAccount.AccountId,
                        senderNewBalance);

                    // Update Receiver Balance
                    UpdateBalance(
                        connection,
                        transaction,
                        receiverAccount.AccountId,
                        receiverNewBalance);

                    // Generate Reference Number
                    long referenceNumber = GenerateReferenceNumber(
                        connection,
                        transaction);

                    // Create Transaction
                    Transaction transactionModel = new Transaction
                    {
                        ReferenceNumber = referenceNumber,
                        TransactionTypeId = 2,       // Transfer
                        TransactionStatusId = 2,     // Completed
                        FromAccountId = senderAccount.AccountId,
                        ToAccountId = receiverAccount.AccountId,
                        Amount = request.Amount,
                        ExchangeRate = 1,
                        Description = "Account Transfer",
                        CreatedBy = 1,               // Admin
                        CreatedAt = DateTime.Now
                    };

                    CreateTransaction(
                        connection,
                        transaction,
                        transactionModel);

                    // Commit
                    transaction.Commit();

                    response.Success = true;
                    response.Message = "Transfer completed successfully.";
                    response.ReferenceNumber = referenceNumber;
                    response.SenderNewBalance = senderNewBalance;
                    response.ReceiverNewBalance = receiverNewBalance;
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
