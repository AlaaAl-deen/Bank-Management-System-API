namespace BankManagementSystem.Modules.Users.Models
{
    public class User
    {
        public int UserId { get; set; }

        public int CustomerNumber { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string SecondName { get; set; } = string.Empty;

        public string ThirdName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public bool MustChangePassword { get; set; }

        public int RoleId { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
