using System.ComponentModel.DataAnnotations;

namespace OnlineMusicStore.Models
{
    public class User
    {
        [Required]
        public string Role { get; set; } = "User"; 

        [Key]
        public int UserId { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        [Display(Name = "Credit Card Number")]
        public string CreditCard { get; set; }
    }
}
