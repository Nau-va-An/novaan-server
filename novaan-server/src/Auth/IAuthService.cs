using System;
using NovaanServer.Auth.DTOs;
using NovaanServer.src.Auth.DTOs;

namespace NovaanServer.Auth
{
	public interface IAuthService
	{
		public Task<bool> SignUpWithCredentials(SignUpDTO signUpDTO);

		public Task<string> SignInWithCredentials(SignInDTOs signInDTO);

		public Task<string> GoogleAuthentication(GoogleOAuthDTO googleOAuthDTO);
	}
}

