using BankManagementSystem.Common;
using BankManagementSystem.Modules.Accounts.Requests;
using BankManagementSystem.Modules.Accounts.Responses;
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

    }
}
