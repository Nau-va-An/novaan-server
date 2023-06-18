using System;
using System.ComponentModel.DataAnnotations;

namespace NovaanServer.src.Auth.DTOs
{
    public class GoogleOAuthDTO
    {
        [Required]
        public string Token { get; set; } = string.Empty;
    }
}

