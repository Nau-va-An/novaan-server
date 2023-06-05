using System;
using Microsoft.AspNetCore.Mvc;
using NovaanServer.Auth.DTOs;
using NovaanServer.src.ExceptionLayer;
using NovaanServer.src.ExceptionLayer.CustomExceptions;

namespace NovaanServer.Auth
{
	[Route("api/auth")]
	[ApiController]
	public class AuthController: Controller
	{
		private readonly IAuthService _authService;

		public AuthController(IAuthService authService) {
			_authService = authService;
		}

		[HttpPost("signup")]
		public async Task<IActionResult> SignUp([FromBody] SignUpDTO signUpDTO)
		{
			await _authService.SignUpWithCredentials(signUpDTO);
			return Ok();
		}

		[HttpPost("signin")]
		public async Task<IActionResult> SignIn()
		{
			return Ok();
		}
	}
}

