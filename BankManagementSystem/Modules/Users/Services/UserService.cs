using BankManagementSystem.Common;
using BankManagementSystem.Database;
using BankManagementSystem.Modules.Users.Requests;
using BankManagementSystem.Modules.Users.Responses;
using System.Data.SqlClient;
using BCrypt.Net;
using BankManagementSystem.Modules.Users.Models;
using BankManagementSystem.Common.Constants;

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


        private int InsertUser(
    User user,
    SqlConnection connection,
    SqlTransaction transaction)
        {
            const string query = @"
    INSERT INTO Users
    (
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
        CreatedAt
    )
    VALUES
    (
        @CustomerNumber,
        @FirstName,
        @SecondName,
        @ThirdName,
        @LastName,
        @PhoneNumber,
        @Address,
        @PasswordHash,
        @MustChangePassword,
        @RoleId,
        @IsActive,
        @CreatedAt
    );

    SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (SqlCommand command = new SqlCommand(query, connection, transaction))
            {
                command.Parameters.AddWithValue("@CustomerNumber", user.CustomerNumber);
                command.Parameters.AddWithValue("@FirstName", user.FirstName);
                command.Parameters.AddWithValue("@SecondName", user.SecondName);
                command.Parameters.AddWithValue("@ThirdName", user.ThirdName);
                command.Parameters.AddWithValue("@LastName", user.LastName);
                command.Parameters.AddWithValue("@PhoneNumber", user.PhoneNumber);
                command.Parameters.AddWithValue("@Address", user.Address);
                command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                command.Parameters.AddWithValue("@MustChangePassword", user.MustChangePassword);
                command.Parameters.AddWithValue("@RoleId", user.RoleId);
                command.Parameters.AddWithValue("@IsActive", user.IsActive);
                command.Parameters.AddWithValue("@CreatedAt", user.CreatedAt);

                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

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

        private long CreateDefaultAccount(
     int userId,
     SqlConnection connection,
     SqlTransaction transaction)
        {
            long accountNumber = GenerateAccountNumber();

            const string query = @"
    INSERT INTO Accounts
    (
        AccountNumber,
        UserId,
        CurrencyId,
        Balance,
        AccountStatusId,
        CreatedAt
    )
    VALUES
    (
        @AccountNumber,
        @UserId,
        @CurrencyId,
        @Balance,
        @AccountStatusId,
        @CreatedAt
    );";

            using (SqlCommand command = new SqlCommand(query, connection, transaction))
            {
                command.Parameters.AddWithValue("@AccountNumber", accountNumber);
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@CurrencyId", 1);
                command.Parameters.AddWithValue("@Balance", 0m);
                command.Parameters.AddWithValue("@AccountStatusId", 1);
                command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

                command.ExecuteNonQuery();
            }

            return accountNumber;
        }

        public CreateUserResponse CreateUser(CreateUserRequest request)
        {
            CreateUserResponse response = new CreateUserResponse();

            try
            {
                // التحقق من البيانات
                ValidateRequest(request);

                // إنشاء كائن المستخدم
                User user = new User
                {
                    CustomerNumber = GenerateCustomerNumber(),

                    FirstName = request.FirstName,
                    SecondName = request.SecondName,
                    ThirdName = request.ThirdName,
                    LastName = request.LastName,

                    PhoneNumber = request.PhoneNumber,
                    Address = request.Address,

                    PasswordHash = HashPassword("123456"),

                    MustChangePassword = true,

                    RoleId = Roles.Customer,

                    IsActive = true,

                    CreatedAt = DateTime.Now
                };

                using (SqlConnection connection = GetConnection())
                {
                    connection.Open();

                    SqlTransaction transaction = connection.BeginTransaction();

                    try
                    {
                        // إنشاء المستخدم
                        int userId = InsertUser(user, connection, transaction);

                        // إنشاء الحساب الافتراضي
                        long accountNumber = CreateDefaultAccount(
                            userId,
                            connection,
                            transaction);

                        // حفظ جميع العمليات
                        transaction.Commit();

                        response.Success = true;
                        response.Message = "Customer created successfully.";

                        response.UserId = userId;
                        response.CustomerNumber = user.CustomerNumber;
                        response.AccountNumber = accountNumber;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }


    }
}
