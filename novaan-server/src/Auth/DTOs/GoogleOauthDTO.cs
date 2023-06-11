using System;
namespace NovaanServer.src.Auth.DTOs
{
	public class GoogleOauthDTO
	{
		// Username
		public string? Name { get; set; }
		// Email
		public string? Email { get; set; }
		// Google ID
		public string? Sub { get; set; }
	}
}

