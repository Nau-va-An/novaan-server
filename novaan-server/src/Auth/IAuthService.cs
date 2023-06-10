using System;
using NovaanServer.Auth.DTOs;

namespace NovaanServer.Auth
{
	public interface IAuthService
	{
		public Task<bool> SignUpWithCredentials(SignUpDTO signUpDTO);
<<<<<<< HEAD
		public Task<bool> SignInWithCredentials(SignInDTOs signInDTO);
=======

		public Task<bool> SignUpGoogle();

		public Task<string> SignInWithCredentials(SignInDTOs signInDTO);
>>>>>>> f79b405 (fix: update refesh token generate flow and refresh flow)

		public Task<bool> GoogleAuthentication(SignUpDTO signUpDTO);
	}
}

