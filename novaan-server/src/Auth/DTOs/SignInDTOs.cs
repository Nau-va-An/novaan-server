using System;
using System.ComponentModel.DataAnnotations;

namespace NovaanServer.Auth.DTOs
{
	public class SignInDTOs
	{
		[Required]
		public string UsernameOrEmail { get; set; }

		[Required]
		public string Password { get; set; }
	}
}

