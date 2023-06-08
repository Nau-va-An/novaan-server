using System;
using NovaanServer.Auth.DTOs;

namespace NovaanServer.Auth
{
	public interface IAuthService
	{
		public Task<bool> SignUpWithCredentials(SignUpDTO signUpDTO);
		public Task<bool> SignInWithCredentials(SignInDTOs signInDTO);

		public Task<bool> GoogleAuthentication(SignUpDTO signUpDTO);
	}
}

