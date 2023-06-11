using System;
using NovaanServer.Auth.DTOs;
using NovaanServer.src.Auth.DTOs;

namespace NovaanServer.Auth
{
	public interface IAuthService
	{
		public Task<bool> SignUpWithCredentials(SignUpDTO signUpDTO);
		public Task<bool> SignInWithCredentials(SignInDTOs signInDTO);

		public Task<bool> GoogleAuthentication(GoogleOauthDTO googleOauthDTO);
	}
}

