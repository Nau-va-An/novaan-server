using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using NovaanServer.Auth.DTOs;
using NovaanServer.src.Auth.DTOs;
using NovaanServer.src.Auth.Jwt;

namespace NovaanServer.Auth
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IJwtService _jwtService;

        public AuthController(IAuthService authService, IJwtService jwtService)
        {
            _authService = authService;
            _jwtService = jwtService;
        }

        [AllowAnonymous]
        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] SignUpDTO signUpDTO)
        {
            await _authService.SignUpWithCredentials(signUpDTO);
            return Ok();
        }

        [AllowAnonymous]
        [HttpPost("signin")]
        public async Task<SignInResDTO> SignIn([FromBody] SignInDTOs signInDTO)
        {
            var userId = await _authService.SignInWithCredentials(signInDTO);
            var token = await _jwtService.GenerateJwtToken(new UserJwt { UserId = userId });

            return new SignInResDTO
            {
                Success = true,
                Token = token
            };
        }

        [Authorize]
        [HttpPost("refreshtoken")]
        public async Task<RefreshTokenResDTO> RefreshToken()
        {
            Request.Headers.TryGetValue(HeaderNames.Authorization, out var authorizationHeader);
            var accessToken = authorizationHeader.ToString()["Bearer ".Length..];
            var newToken = await _jwtService.RefreshToken(accessToken);

            return new RefreshTokenResDTO
            {
                Success = true,
                Token = newToken
            };
        }

        [HttpPost("google")]
        public async Task<SignInResDTO> GoogleAuthentication([FromBody] GoogleOAuthDTO googleOAuthDTO)
        {
            var userId = await _authService.GoogleAuthentication(googleOAuthDTO);
            var token = await _jwtService.GenerateJwtToken(new UserJwt { UserId = userId });
            return new SignInResDTO
            {
                Success = true,
                Token = token
            };
        }
    }
}

