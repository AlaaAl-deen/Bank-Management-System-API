using Microsoft.Data.SqlClient;
using System;

namespace BankManagementSystem.Database
{
    public class DatabaseTest
    {
        public void TestConnection()
        {
            DatabaseConnection database = new DatabaseConnection();

            using (SqlConnection connection = database.GetConnection())
            {
                try
                {
                    connection.Open();

                    Console.WriteLine("Database Connected Successfully.");

                    connection.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Connection Failed.");

                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}