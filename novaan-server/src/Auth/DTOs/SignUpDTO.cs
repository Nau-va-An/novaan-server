using System;
using System.ComponentModel.DataAnnotations;

namespace NovaanServer.Auth.DTOs
{
    public class SignUpDTO
    {
        [Required]
        [MinLength(5)] // Facebook min username length
        public string Username { get; set; } = "";

        [Required]
        [MinLength(8)] // Recommended minimum length by NIST
        public string Password { get; set; } = "";

        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";
    }
}

