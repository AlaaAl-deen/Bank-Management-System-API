using System;
using System.Data.SqlClient;
using BCrypt.Net;
using BankManagementSystem.Database;

namespace BankManagementSystem.Database
{
    public class DatabaseSeeder : DatabaseConnection
    {
        public void SeedAdmin()
        {
            using SqlConnection connection = GetConnection();

            connection.Open();

            string checkQuery = @"
                SELECT COUNT(*)
                FROM Users
                WHERE RoleId = 1";

            using SqlCommand checkCommand = new SqlCommand(checkQuery, connection);

            int adminCount = Convert.ToInt32(checkCommand.ExecuteScalar());

            if (adminCount > 0)
                return;

            string passwordHash = BCrypt.Net.BCrypt.HashPassword("Admin123");

            string insertQuery = @"
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
                )";

            using SqlCommand command = new SqlCommand(insertQuery, connection);

            command.Parameters.AddWithValue("@CustomerNumber", 100000);

            command.Parameters.AddWithValue("@FirstName", "System");
            command.Parameters.AddWithValue("@SecondName", "Administrator");
            command.Parameters.AddWithValue("@ThirdName", "");
            command.Parameters.AddWithValue("@LastName", "");

            command.Parameters.AddWithValue("@PhoneNumber", "700000000");
            command.Parameters.AddWithValue("@Address", "Head Office");

            command.Parameters.AddWithValue("@PasswordHash", passwordHash);

            command.Parameters.AddWithValue("@MustChangePassword", false);

            command.Parameters.AddWithValue("@RoleId", 1);

            command.Parameters.AddWithValue("@IsActive", true);

            command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

            command.ExecuteNonQuery();
        }
    }
}