using System;
using System.ComponentModel.DataAnnotations;

namespace NovaanServer.Auth.DTOs
{
    public class SignUpDTO
    {
        [Required]
        [MaxLength(64)]
        public string DisplayName { get; set; } = "";

        [Required]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$",
        ErrorMessage = "Password must be at least 8 characters long and contain at least one lowercase letter, one uppercase letter, and one number.")]
        public string Password { get; set; } = "";

        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";
    }
}

