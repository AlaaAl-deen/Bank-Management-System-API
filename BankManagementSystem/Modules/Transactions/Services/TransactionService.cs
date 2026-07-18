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
                            UpdatedAt = reader["UpdatedAt"] == DBNull.Value ? null : Convert.ToDateTime(reader["UpdatedAt"])
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


        public TransferResponse Transfer(
     TransferRequest request,
     int? customerNumber = null)
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

                    // التحقق من ملكية الحساب إذا كان الطلب من العميل
                    if (customerNumber.HasValue)
                    {
                        bool isOwner = IsAccountOwnedByCustomer(
                            connection,
                            transaction,
                            request.FromAccountNumber,
                            customerNumber.Value);

                        if (!isOwner)
                        {
                            throw new Exception(
                                "You are not authorized to use this account.");
                        }
                    }

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
                        throw new Exception(
                            "Transfer is allowed only between accounts with the same currency. Please use the Exchange service.");
                    }

                    // Check Balance
                    if (!HasSufficientBalance(senderAccount, request.Amount))
                    {
                        throw new Exception("Insufficient balance.");
                    }

                    // Calculate New Balances
                    decimal senderNewBalance =
                        senderAccount.Balance - request.Amount;

                    decimal receiverNewBalance =
                        receiverAccount.Balance + request.Amount;

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
                    long referenceNumber =
                        GenerateReferenceNumber(
                            connection,
                            transaction);

                    // Create Transaction
                    Transaction transactionModel = new Transaction
                    {
                        ReferenceNumber = referenceNumber,
                        TransactionTypeId = 2,
                        TransactionStatusId = 2,
                        FromAccountId = senderAccount.AccountId,
                        ToAccountId = receiverAccount.AccountId,
                        Amount = request.Amount,
                        ExchangeRate = 1,
                        Description = "Account Transfer",
                        CreatedBy = 1,
                        CreatedAt = DateTime.Now
                    };

                    CreateTransaction(
                        connection,
                        transaction,
                        transactionModel);

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

        //exchange 
        private void ValidateExchangeRequest(ExchangeRequest request)
        {
            if (request == null)
                throw new Exception("Request cannot be null.");

            if (request.FromAccountNumber <= 0)
                throw new Exception("Source account number is required.");

            if (request.ToAccountNumber <= 0)
                throw new Exception("Destination account number is required.");

            if (request.FromAccountNumber == request.ToAccountNumber)
                throw new Exception("Source and destination accounts cannot be the same.");

            if (request.Amount <= 0)
                throw new Exception("Amount must be greater than zero.");
        }

        private decimal GetExchangeRate(
    SqlConnection connection,
    SqlTransaction transaction,
    int fromCurrencyId,
    int toCurrencyId)
        {
            const string query = @"
    SELECT ExchangeRate
    FROM ExchangeRates
    WHERE FromCurrencyId = @FromCurrencyId
      AND ToCurrencyId = @ToCurrencyId";

            using (SqlCommand command = new SqlCommand(query, connection, transaction))
            {
                command.Parameters.AddWithValue("@FromCurrencyId", fromCurrencyId);
                command.Parameters.AddWithValue("@ToCurrencyId", toCurrencyId);

                object result = command.ExecuteScalar();

                if (result == null || result == DBNull.Value)
                {
                    throw new Exception("Exchange rate not found.");
                }

                return Convert.ToDecimal(result);
            }
        }

        public ExchangeResponse Exchange(
    ExchangeRequest request,
    int? customerNumber = null)
        {
            ExchangeResponse response = new ExchangeResponse();

            using (SqlConnection connection = GetConnection())
            {
                connection.Open();

                SqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    // Validate Request
                    ValidateExchangeRequest(request);

                    // التحقق من ملكية الحساب إذا كان الطلب من العميل
                    if (customerNumber.HasValue)
                    {
                        bool isOwner = IsAccountOwnedByCustomer(
                            connection,
                            transaction,
                            request.FromAccountNumber,
                            customerNumber.Value);

                        if (!isOwner)
                        {
                            throw new Exception(
                                "You are not authorized to use this account.");
                        }
                    }

                    // Get Source Account
                    Account sourceAccount = GetAccountByAccountNumber(
                        connection,
                        transaction,
                        request.FromAccountNumber);

                    if (sourceAccount == null)
                    {
                        throw new Exception("Source account not found.");
                    }

                    // Get Destination Account
                    Account destinationAccount = GetAccountByAccountNumber(
                        connection,
                        transaction,
                        request.ToAccountNumber);

                    if (destinationAccount == null)
                    {
                        throw new Exception("Destination account not found.");
                    }

                    // Both accounts must belong to the same customer
                    if (sourceAccount.UserId != destinationAccount.UserId)
                    {
                        throw new Exception(
                            "Exchange is allowed only between accounts belonging to the same customer.");
                    }

                    // Accounts must have different currencies
                    if (sourceAccount.CurrencyId == destinationAccount.CurrencyId)
                    {
                        throw new Exception(
                            "Source and destination accounts must have different currencies.");
                    }

                    // Check Account Status
                    if (!IsAccountActive(sourceAccount.AccountStatusId))
                    {
                        throw new Exception("Source account is inactive.");
                    }

                    if (!IsAccountActive(destinationAccount.AccountStatusId))
                    {
                        throw new Exception("Destination account is inactive.");
                    }

                    // Check Balance
                    if (!HasSufficientBalance(sourceAccount, request.Amount))
                    {
                        throw new Exception("Insufficient balance.");
                    }

                    // Get Exchange Rate
                    decimal exchangeRate = GetExchangeRate(
                        connection,
                        transaction,
                        sourceAccount.CurrencyId,
                        destinationAccount.CurrencyId);

                    // Calculate Converted Amount
                    decimal convertedAmount =
                        request.Amount * exchangeRate;

                    // Calculate New Balances
                    decimal sourceNewBalance =
                        sourceAccount.Balance - request.Amount;

                    decimal destinationNewBalance =
                        destinationAccount.Balance + convertedAmount;

                    // Update Source Balance
                    UpdateBalance(
                        connection,
                        transaction,
                        sourceAccount.AccountId,
                        sourceNewBalance);

                    // Update Destination Balance
                    UpdateBalance(
                        connection,
                        transaction,
                        destinationAccount.AccountId,
                        destinationNewBalance);

                    // Generate Reference Number
                    long referenceNumber =
                        GenerateReferenceNumber(
                            connection,
                            transaction);

                    // Create Transaction
                    Transaction transactionModel = new Transaction
                    {
                        ReferenceNumber = referenceNumber,
                        TransactionTypeId = 3,
                        TransactionStatusId = 2,
                        FromAccountId = sourceAccount.AccountId,
                        ToAccountId = destinationAccount.AccountId,
                        Amount = request.Amount,
                        ExchangeRate = exchangeRate,
                        Description = "Currency Exchange",
                        CreatedBy = 1,
                        CreatedAt = DateTime.Now
                    };

                    CreateTransaction(
                        connection,
                        transaction,
                        transactionModel);

                    transaction.Commit();

                    response.Success = true;
                    response.Message = "Exchange completed successfully.";
                    response.ReferenceNumber = referenceNumber;
                    response.ExchangeRate = exchangeRate;
                    response.ConvertedAmount = convertedAmount;
                    response.SenderNewBalance = sourceNewBalance;
                    response.ReceiverNewBalance = destinationNewBalance;
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

        //Account statement
        private void ValidateAccountNumber(long accountNumber)
        {
            if (accountNumber <= 0)
            {
                throw new Exception("Account number is required.");
            }
        }

        private List<StatementTransactionResponse> GetTransactionsByAccount(
        SqlConnection connection,
        SqlTransaction transaction,
        int accountId)
        {
            List<StatementTransactionResponse> transactions = new List<StatementTransactionResponse>();

            const string query = @"
    SELECT
        t.ReferenceNumber,
        tt.TransactionTypeName,
        t.FromAccountId,
        t.ToAccountId,
        t.Amount,
        t.ExchangeRate,
        t.Description,
        t.CreatedAt
    FROM Transactions t
    INNER JOIN TransactionTypes tt
        ON t.TransactionTypeId = tt.TransactionTypeId
    WHERE
        t.FromAccountId = @AccountId
        OR t.ToAccountId = @AccountId
    ORDER BY t.CreatedAt DESC;";

            using (SqlCommand command = new SqlCommand(query, connection, transaction))
            {
                command.Parameters.AddWithValue("@AccountId", accountId);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        StatementTransactionResponse statement = new StatementTransactionResponse
                        {
                            ReferenceNumber = Convert.ToInt64(reader["ReferenceNumber"]),
                            TransactionType = reader["TransactionTypeName"].ToString(),
                            Amount = Convert.ToDecimal(reader["Amount"]),
                            ExchangeRate = Convert.ToDecimal(reader["ExchangeRate"]),
                            Description = reader["Description"].ToString(),
                            CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                        };

                        int? fromAccountId = reader["FromAccountId"] == DBNull.Value
                            ? (int?)null
                            : Convert.ToInt32(reader["FromAccountId"]);

                        int? toAccountId = reader["ToAccountId"] == DBNull.Value
                            ? (int?)null
                            : Convert.ToInt32(reader["ToAccountId"]);

                        switch (statement.TransactionType)
                        {
                            case "Deposit":
                                statement.TransactionDirection = "Deposit";
                                break;

                            case "Transfer":
                                statement.TransactionDirection =
                                    fromAccountId == accountId
                                        ? "Transfer Out"
                                        : "Transfer In";
                                break;

                            case "Exchange":
                                statement.TransactionDirection =
                                    fromAccountId == accountId
                                        ? "Exchange Out"
                                        : "Exchange In";
                                break;

                            default:
                                statement.TransactionDirection = "Unknown";
                                break;
                        }

                        transactions.Add(statement);
                    }
                }
            }

            return transactions;
        }

        private AccountStatementInfo GetAccountStatementInfo(
    SqlConnection connection,
    SqlTransaction transaction,
    long accountNumber)
        {
            const string query = @"
    SELECT
        a.AccountId,
        a.AccountNumber,
        u.CustomerNumber,
        CONCAT(
            u.FirstName, ' ',
            u.SecondName, ' ',
            u.ThirdName, ' ',
            u.LastName
        ) AS CustomerName,
        c.CurrencyCode,
        a.Balance
    FROM Accounts a
    INNER JOIN Users u
        ON a.UserId = u.UserId
    INNER JOIN Currencies c
        ON a.CurrencyId = c.CurrencyId
    WHERE a.AccountNumber = @AccountNumber;";

            using (SqlCommand command = new SqlCommand(query, connection, transaction))
            {
                command.Parameters.AddWithValue("@AccountNumber", accountNumber);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new AccountStatementInfo
                        {
                            AccountId = Convert.ToInt32(reader["AccountId"]),
                            AccountNumber = Convert.ToInt64(reader["AccountNumber"]),
                            CustomerNumber = Convert.ToInt32(reader["CustomerNumber"]),
                            CustomerName = reader["CustomerName"].ToString(),
                            Currency = reader["CurrencyCode"].ToString(),
                            Balance = Convert.ToDecimal(reader["Balance"])
                        };
                    }
                }
            }

            return null;
        }

        public AccountStatementResponse GetAccountStatement(long accountNumber)
        {
            ValidateAccountNumber(accountNumber);

            using (SqlConnection connection = GetConnection())
            {
                connection.Open();

                SqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    AccountStatementInfo accountInfo =
                        GetAccountStatementInfo(connection, transaction, accountNumber);

                    if (accountInfo == null)
                    {
                        throw new Exception("Account not found.");
                    }

                    List<StatementTransactionResponse> transactions =
                        GetTransactionsByAccount(
                            connection,
                            transaction,
                            accountInfo.AccountId);

                    AccountStatementResponse response = new AccountStatementResponse
                    {
                        Success = true,
                        Message = "Account statement retrieved successfully.",

                        AccountNumber = accountInfo.AccountNumber,
                        CustomerNumber = accountInfo.CustomerNumber,
                        CustomerName = accountInfo.CustomerName,
                        Currency = accountInfo.Currency,
                        CurrentBalance = accountInfo.Balance,

                        Transactions = transactions
                    };

                    transaction.Commit();

                    return response;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        //Customer 
        public AccountStatementResponse GetMyAccountStatement(
    int customerNumber,
    long accountNumber)
        {
            ValidateAccountNumber(accountNumber);

            using (SqlConnection connection = GetConnection())
            {
                connection.Open();

                SqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    AccountStatementInfo accountInfo =
                        GetAccountStatementInfo(
                            connection,
                            transaction,
                            accountNumber);

                    if (accountInfo == null)
                    {
                        throw new Exception("Account not found.");
                    }

                    // التأكد أن الحساب يخص العميل المسجل دخوله
                    if (accountInfo.CustomerNumber != customerNumber)
                    {
                        throw new Exception("You are not authorized to access this account.");
                    }

                    List<StatementTransactionResponse> transactions =
                        GetTransactionsByAccount(
                            connection,
                            transaction,
                            accountInfo.AccountId);

                    AccountStatementResponse response =
                        new AccountStatementResponse
                        {
                            Success = true,
                            Message = "Account statement retrieved successfully.",

                            AccountNumber = accountInfo.AccountNumber,
                            CustomerNumber = accountInfo.CustomerNumber,
                            CustomerName = accountInfo.CustomerName,
                            Currency = accountInfo.Currency,
                            CurrentBalance = accountInfo.Balance,

                            Transactions = transactions
                        };

                    transaction.Commit();

                    return response;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        //trnsfer for customer
        private bool IsAccountOwnedByCustomer(
    SqlConnection connection,
    SqlTransaction transaction,
    long accountNumber,
    int customerNumber)
        {
            const string query = @"
        SELECT COUNT(*)
        FROM Accounts A
        INNER JOIN Users U
            ON A.UserId = U.UserId
        WHERE
            A.AccountNumber = @AccountNumber
        AND
            U.CustomerNumber = @CustomerNumber";

            using SqlCommand command =
                new SqlCommand(query, connection, transaction);

            command.Parameters.AddWithValue("@AccountNumber", accountNumber);
            command.Parameters.AddWithValue("@CustomerNumber", customerNumber);

            return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }

    }
}
