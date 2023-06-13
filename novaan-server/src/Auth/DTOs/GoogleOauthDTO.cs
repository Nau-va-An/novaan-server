using System;
using System.ComponentModel.DataAnnotations;

namespace NovaanServer.src.Auth.DTOs
{
	public class GoogleOauthDTO
	{
		[Required]
		// Username
		public string Name { get; set; }
		[Required]
		// Email
		public string Email { get; set; }
		[Required]
		// Google ID
		public string Sub { get; set; }
	}
}

