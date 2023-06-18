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
        [MinLength(8)] // Recommended minimum length by NIST
        public string Password { get; set; } = "";

        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";
    }
}

