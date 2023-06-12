using System;
using NovaanServer.src.Auth.DTOs;

namespace NovaanServer.src.Auth.Jwt
{
	public interface IJwtService
	{
		public Task<string> GenerateJwtToken(UserJwt userToken);

		public Task<string> RefreshToken(string accessToken);
    }
}

