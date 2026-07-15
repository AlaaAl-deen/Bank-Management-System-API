using BankManagementSystem.Common;
using BankManagementSystem.Modules.Authentication.Requests;
using BankManagementSystem.Modules.Authentication.Responses;
using BankManagementSystem.Modules.Users.Models;
using System.Data.SqlClient;

namespace BankManagementSystem.Modules.Authentication.Services
{
    public class AuthenticationService : BaseService
    {

        private void ValidateRequest(LoginRequest request)
        {
            if (request == null)
                throw new Exception("Request cannot be null.");

            if (request.CustomerNumber <= 0)
                throw new Exception("Customer number is required.");

            if (string.IsNullOrWhiteSpace(request.Password))
                throw new Exception("Password is required.");
        }

        public LoginResponse Login(LoginRequest request)
        {
            LoginResponse response = new LoginResponse();

            try
            {
                // 1. التحقق من صحة البيانات
                ValidateRequest(request);

                // 2. البحث عن المستخدم
                User? user = GetUserByCustomerNumber(request.CustomerNumber);

                if (user == null)
                    throw new Exception("Customer not found.");

                // 3. التحقق من كلمة المرور

                if (!VerifyPassword(request.Password, user.PasswordHash))
                {
                    throw new Exception("Invalid password.");
                }

                // 4. التحقق من حالة الحساب
                CheckUserStatus(user);

                // 5. إنشاء الاستجابة

                response.Success = true;
                response.Message = "Login successful.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }

        private User? GetUserByCustomerNumber(int customerNumber)
        {
            using (SqlConnection connection = GetConnection())
            {
                connection.Open();

                const string query = @"
            SELECT
                UserId,
                CustomerNumber,
                FirstName,
                SecondName,
                ThirdName,
                LastName,
                PhoneNumber,
                Address,
                PasswordHash,
                MustChangePassword,
                RoleId,
                IsActive,
                CreatedAt,
                UpdatedAt
            FROM Users
            WHERE CustomerNumber = @CustomerNumber";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CustomerNumber", customerNumber);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                            return null;

                        return new User
                        {
                            UserId = Convert.ToInt32(reader["UserId"]),
                            CustomerNumber = Convert.ToInt32(reader["CustomerNumber"]),
                            FirstName = reader["FirstName"].ToString()!,
                            SecondName = reader["SecondName"].ToString()!,
                            ThirdName = reader["ThirdName"].ToString()!,
                            LastName = reader["LastName"].ToString()!,
                            PhoneNumber = reader["PhoneNumber"].ToString()!,
                            Address = reader["Address"].ToString()!,
                            PasswordHash = reader["PasswordHash"].ToString()!,
                            MustChangePassword = Convert.ToBoolean(reader["MustChangePassword"]),
                            RoleId = Convert.ToInt32(reader["RoleId"]),
                            IsActive = Convert.ToBoolean(reader["IsActive"]),
                            CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                            UpdatedAt = reader["UpdatedAt"] == DBNull.Value
                                ? null
                                : Convert.ToDateTime(reader["UpdatedAt"])
                        };
                    }
                }
            }
        }
        private bool VerifyPassword(string password, string passwordHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }

        private void CheckUserStatus(User user)
        {
            if (!user.IsActive)
            {
                throw new Exception("This account is inactive.");
            }
        }



    }

  
}