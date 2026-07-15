namespace BankManagementSystem.Modules.Users.Models
{
    public class User
    {
        public int UserId { get; set; }

        public int CustomerNumber { get; set; }

        public string FirstName { get; set; }

        public string SecondName { get; set; }

        public string ThirdName { get; set; }

        public string LastName { get; set; }

        public string PhoneNumber { get; set; }

        public string Address { get; set; }

        public string PasswordHash { get; set; }

        public bool MustChangePassword { get; set; }

        public int RoleId { get; set; }

        public bool IsActive { get; set; }
    }
}
