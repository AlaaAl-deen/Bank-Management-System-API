using BankManagementSystem.Common;
using BankManagementSystem.Database;
using BankManagementSystem.Modules.Users.Requests;
using BankManagementSystem.Modules.Users.Responses;
using Microsoft.Data.SqlClient;
using BCrypt.Net;

namespace BankManagementSystem.Modules.Users.Services
{
    public class UserService : BaseService
    {
        private void ValidateRequest(CreateUserRequest request)
        {
            if (request == null)
                throw new Exception("Request cannot be null.");

            if (string.IsNullOrWhiteSpace(request.FirstName))
                throw new Exception("First name is required.");

            if (string.IsNullOrWhiteSpace(request.SecondName))
                throw new Exception("Second name is required.");

            if (string.IsNullOrWhiteSpace(request.ThirdName))
                throw new Exception("Third name is required.");

            if (string.IsNullOrWhiteSpace(request.LastName))
                throw new Exception("Last name is required.");

            if (string.IsNullOrWhiteSpace(request.PhoneNumber))
                throw new Exception("Phone number is required.");

            if (string.IsNullOrWhiteSpace(request.Address))
                throw new Exception("Address is required.");

            if (IsPhoneNumberExists(request.PhoneNumber))
                throw new Exception("Phone number already exists.");
        }

        private bool IsPhoneNumberExists(string phoneNumber)
        {
            using (SqlConnection connection = GetConnection())
            {
                connection.Open();

                string query = @"SELECT COUNT(*) FROM Users WHERE PhoneNumber = @PhoneNumber";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@PhoneNumber", phoneNumber);

                    int count = Convert.ToInt32(command.ExecuteScalar());

                    return count > 0;
                }
            }
        }

        private int GenerateCustomerNumber()
        {
            using (SqlConnection connection = GetConnection())
            {
                connection.Open();

                const string query = @"SELECT MAX(CustomerNumber) FROM Users";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    object result = command.ExecuteScalar();

                    if (result == DBNull.Value || result == null)
                    {
                        return 100001;
                    }

                    return Convert.ToInt32(result) + 1;
                }
            }

        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }


    }
}
