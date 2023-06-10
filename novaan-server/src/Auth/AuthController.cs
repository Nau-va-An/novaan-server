using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Amazon.Runtime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Microsoft.Net.Http;
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
using System.Net;

namespace NovaanServer.Auth
{
    [AllowAnonymous]
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

        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] SignUpDTO signUpDTO)
        {
            await _authService.SignUpWithCredentials(signUpDTO);
            return Ok();
        }

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

        [HttpPost("refreshtoken")]
        public async Task<RefreshTokenResDTO> RefreshToken()
        {
            Request.Headers.TryGetValue(HeaderNames.Authorization, out var authorizationHeader);
            var accessToken = authorizationHeader.ToString().Substring("Bearer ".Length);
            var newToken = await _jwtService.RefreshToken(accessToken);

            return new RefreshTokenResDTO
            {
                Success = true,
                Token = newToken
            };
        }
    }
}

