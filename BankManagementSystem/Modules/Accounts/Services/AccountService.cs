using BankManagementSystem.Common;
using BankManagementSystem.Modules.Accounts.Requests;
using BankManagementSystem.Modules.Accounts.Responses;
using BankManagementSystem.Modules.Users.Models;
using System.Data.SqlClient;

namespace BankManagementSystem.Modules.Accounts.Services
{
    public class AccountService : BaseService
    {
        private List<AccountResponse> GetAccountsByCustomerNumber(int customerNumber)
        {
            List<AccountResponse> accounts = new List<AccountResponse>();

            using (SqlConnection connection = GetConnection())
            {
                connection.Open();

                const string query = @"
        SELECT
            a.AccountNumber,
            c.CurrencyCode,
            a.Balance,
            s.StatusName
        FROM Accounts a
        INNER JOIN Users u
            ON a.UserId = u.UserId
        INNER JOIN Currencies c
            ON a.CurrencyId = c.CurrencyId
        INNER JOIN AccountStatuses s
            ON a.AccountStatusId = s.AccountStatusId
        WHERE u.CustomerNumber = @CustomerNumber";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CustomerNumber", customerNumber);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            AccountResponse account = new AccountResponse
                            {
                                AccountNumber = Convert.ToInt64(reader["AccountNumber"]),
                                CurrencyCode = reader["CurrencyCode"].ToString()!,
                                Balance = Convert.ToDecimal(reader["Balance"]),
                                Status = reader["StatusName"].ToString()!
                            };

                            accounts.Add(account);
                        }
                    }
                }
            }

            return accounts;
        }
        public GetAccountsResponse GetAccounts(int customerNumber)
        {
            GetAccountsResponse response = new GetAccountsResponse();

            try
            {
                if (customerNumber <= 0)
                {
                    throw new Exception("Customer number is required.");
                }

                List<AccountResponse> accounts =
                    GetAccountsByCustomerNumber(customerNumber);

                response.Success = true;
                response.Accounts = accounts;

                if (accounts.Count == 0)
                {
                    response.Message = "No accounts found for this customer.";
                }
                else
                {
                    response.Message = "Accounts retrieved successfully.";
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }

        private void ValidateAddCurrencyAccountRequest(AddCurrencyAccountRequest request)
        {
            if (request == null)
            {
                throw new Exception("Request cannot be null.");
            }

            if (request.CustomerNumber <= 0)
            {
                throw new Exception("Customer number is required.");
            }

            if (request.CurrencyId <= 0)
            {
                throw new Exception("Currency is required.");
            }
        }

        private bool IsCurrencyExists(int currencyId)
        {
            using (SqlConnection connection = GetConnection())
            {
                connection.Open();

                const string query = @"
        SELECT COUNT(*)
        FROM Currencies
        WHERE CurrencyId = @CurrencyId";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CurrencyId", currencyId);

                    int count = Convert.ToInt32(command.ExecuteScalar());

                    return count > 0;
                }
            }
        }

        private bool IsCurrencyActive(int currencyId)
        {
            using (SqlConnection connection = GetConnection())
            {
                connection.Open();

                const string query = @"
        SELECT IsActive
        FROM Currencies
        WHERE CurrencyId = @CurrencyId";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CurrencyId", currencyId);

                    object result = command.ExecuteScalar();

                    if (result == null)
                    {
                        return false;
                    }

                    return Convert.ToBoolean(result);
                }
            }
        }

        private bool CustomerHasCurrency(int customerNumber, int currencyId)
        {
            using (SqlConnection connection = GetConnection())
            {
                connection.Open();

                const string query = @"
        SELECT COUNT(*)
        FROM Accounts a
        INNER JOIN Users u
            ON a.UserId = u.UserId
        WHERE u.CustomerNumber = @CustomerNumber
        AND a.CurrencyId = @CurrencyId";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CustomerNumber", customerNumber);
                    command.Parameters.AddWithValue("@CurrencyId", currencyId);

                    int count = Convert.ToInt32(command.ExecuteScalar());

                    return count > 0;
                }
            }
        }

        //We can reuseing
        private long GenerateAccountNumber()
        {
            using (SqlConnection connection = GetConnection())
            {
                connection.Open();

                const string query = @"
        SELECT MAX(AccountNumber)
        FROM Accounts";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    object result = command.ExecuteScalar();

                    if (result == DBNull.Value || result == null)
                    {
                        return 1000000001;
                    }

                    return Convert.ToInt64(result) + 1;
                }
            }
        }

        private void CreateAccount(long accountNumber, int userId, int currencyId)
        {
            using (SqlConnection connection = GetConnection())
            {
                connection.Open();

                const string query = @"
        INSERT INTO Accounts
        (
            AccountNumber,
            UserId,
            CurrencyId,
            Balance,
            AccountStatusId,
            CreatedAt,
            UpdatedAt
        )
        VALUES
        (
            @AccountNumber,
            @UserId,
            @CurrencyId,
            @Balance,
            @AccountStatusId,
            @CreatedAt,
            @UpdatedAt
        )";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@AccountNumber", accountNumber);
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@CurrencyId", currencyId);
                    command.Parameters.AddWithValue("@Balance", 0);
                    command.Parameters.AddWithValue("@AccountStatusId", 1);
                    command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                    command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);

                    command.ExecuteNonQuery();
                }
            }
        }

        public AddCurrencyAccountResponse AddCurrencyAccount(AddCurrencyAccountRequest request)
        {
            AddCurrencyAccountResponse response = new AddCurrencyAccountResponse();

            try
            {
                ValidateAddCurrencyAccountRequest(request);

                User user = GetUserByCustomerNumber(request.CustomerNumber);

                if (user == null)
                {
                    throw new Exception("Customer not found.");
                }

                if (!IsCurrencyExists(request.CurrencyId))
                {
                    throw new Exception("Currency not found.");
                }

                if (!IsCurrencyActive(request.CurrencyId))
                {
                    throw new Exception("This currency is currently inactive.");
                }

                if (CustomerHasCurrency(request.CustomerNumber, request.CurrencyId))
                {
                    throw new Exception("Customer already has an account with this currency.");
                }

                long accountNumber = GenerateAccountNumber();

                CreateAccount(
                    accountNumber,
                    user.UserId,
                    request.CurrencyId);

                response.Success = true;
                response.Message = "Account created successfully.";
                response.AccountNumber = accountNumber;

                response.CurrencyCode = GetCurrencyCode(request.CurrencyId);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }

        //reuse in Auth service 
        private User GetUserByCustomerNumber(int customerNumber)
        {
            using (SqlConnection connection = GetConnection())
            {
                connection.Open();

                const string query = @"
        SELECT
            UserId,
            CustomerNumber,
            RoleId,
            IsActive
        FROM Users
        WHERE CustomerNumber = @CustomerNumber";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CustomerNumber", customerNumber);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new User
                            {
                                UserId = Convert.ToInt32(reader["UserId"]),
                                CustomerNumber = Convert.ToInt32(reader["CustomerNumber"]),
                                RoleId = Convert.ToInt32(reader["RoleId"]),
                                IsActive = Convert.ToBoolean(reader["IsActive"])
                            };
                        }
                    }
                }
            }

            return null;
        }

        private string GetCurrencyCode(int currencyId)
        {
            using (SqlConnection connection = GetConnection())
            {
                connection.Open();

                const string query = @"
        SELECT CurrencyCode
        FROM Currencies
        WHERE CurrencyId = @CurrencyId";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CurrencyId", currencyId);

                    object result = command.ExecuteScalar();

                    if (result == null || result == DBNull.Value)
                    {
                        return string.Empty;
                    }

                    return result.ToString();
                }
            }
        }

    }
}
