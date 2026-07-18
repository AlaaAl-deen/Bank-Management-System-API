namespace BankManagementSystem.Modules.Users.Responses
{
    public class CustomerResponse
    {
        public int CustomerNumber { get; set; }

        public string FullName { get; set; }

        public string PhoneNumber { get; set; }

        public string Address { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
