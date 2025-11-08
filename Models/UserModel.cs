using System.ComponentModel.DataAnnotations;

namespace HMS.Models
{
    public class UserModel
    {
        /*CREATE TABLE [User]
          (
              UserID INT PRIMARY KEY IDENTITY(1,1),
              UserName NVARCHAR(100) NOT NULL,
              Password NVARCHAR(100) NOT NULL,
              Email NVARCHAR(100) NOT NULL,
              MobileNo NVARCHAR(100) NOT NULL,
              IsActive BIT NOT NULL DEFAULT 1,
              Created DATETIME DEFAULT GETDATE(),
              Modified DATETIME NOT NULL,
              ProfileImage NVARCHAR(255) NULL DEFAULT NULL
          );
         */

        public int UserID { get; set; }

        [Required(ErrorMessage = "Username is required.")]
        [StringLength(100, ErrorMessage = "Username cannot exceed 100 characters.")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, ErrorMessage = "Password cannot exceed 100 characters.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
ErrorMessage = "Password must be at least 8 characters, include one uppercase, one lowercase, one number, and one special character.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mobile number is required.")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Mobile number must be exactly 10 digits.")]
        public string MobileNo { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime Created { get; set; } = DateTime.Now;
        public DateTime Modified { get; set; } = DateTime.Now;
        public IFormFile? ProfileImage { get; set; }
        public string? ProfileImagePath { get; set; }

    }
}
