﻿using System;
using System.ComponentModel.DataAnnotations;

namespace NovaanServer.Auth.DTOs
{
    public class SignInDTOs
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}

