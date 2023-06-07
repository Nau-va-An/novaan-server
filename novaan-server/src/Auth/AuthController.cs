using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Amazon.Runtime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoConnector;
using MongoConnector.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using NovaanServer.Auth.DTOs;
using NovaanServer.Configuration;
using NovaanServer.src.Auth.DTOs;
using NovaanServer.src.Auth.Jwt;
using NovaanServer.src.ExceptionLayer;
using NovaanServer.src.ExceptionLayer.CustomExceptions;

namespace NovaanServer.Auth
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : Controller
    {      
        private readonly IAuthService _authService;
        private readonly JwtService _jwtService;

        public AuthController(IAuthService authService, JwtService jwtService)
        {
            _authService = authService;
            _jwtService = jwtService;       
        }

        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] SignUpDTO signUpDTO)
        {
            await _authService.SignUpWithCredentials(signUpDTO);
            return Ok();
        }

        [HttpPost("signin")]
        public async Task<IActionResult> SignIn([FromBody] SignInDTOs signInDTO)
        {
            await _authService.SignInWithCredentials(signInDTO);

            var token = await _jwtService.GennerateJwtToken(new UserJwt
            {
                Email = signInDTO.UsernameOrEmail
            });

            return Ok(token);
        }

        [HttpPost("refreshtoken")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequest tokenRequest)
        {
            var result = await _jwtService.VerifyAndGennerateToken(tokenRequest);

            if(result == null)
            {
                return BadRequest(new SignInResponseDTO()
                {
                    Errors = new List<string>
                {
                    "Invalid tokens"
                },
                    Result = false
                });
            }
            return Ok(result);
        }
    }
}

