using System;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using MongoConnector;
using MongoConnector.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using NovaanServer.Auth.DTOs;
using NovaanServer.src.Auth.DTOs;
using NovaanServer.src.ExceptionLayer.CustomExceptions;
using Utils.Hash;

namespace NovaanServer.Auth
{
    public class AuthService : IAuthService
    {
        private readonly MongoDBService _mongoService;

        public AuthService(MongoDBService mongoDBService)
        {
            _mongoService = mongoDBService;
        }

        // Sign in and return userId
        public async Task<string> SignInWithCredentials(SignInDTOs signInDTO)
        {
            var foundUser = (await _mongoService.Accounts
                 .FindAsync(
                    acc => acc.Email == signInDTO.UsernameOrEmail ||
                     acc.Username == signInDTO.UsernameOrEmail
                 ))
                 .FirstOrDefault()
                 ?? throw new BadHttpRequestException(ExceptionMessage.EMAIL_OR_PASSWORD_NOT_FOUND);

            var hashPassword = CustomHash.GetHashString(signInDTO.Password);
            if (foundUser.Password != hashPassword)
            {
                throw new BadHttpRequestException(ExceptionMessage.EMAIL_OR_PASSWORD_NOT_FOUND);
            }

            return foundUser.Id;
        }

        public async Task<bool> SignUpWithCredentials(SignUpDTO signUpDTO)
        {
            var emailExisted = await CheckEmailExist(signUpDTO.Email);
            if (emailExisted)
            {
                throw new BadHttpRequestException(ExceptionMessage.EMAIL_TAKEN);
            }

            var usernameExisted = await CheckUsernameExist(signUpDTO.Username);
            if (usernameExisted)
            {
                throw new BadHttpRequestException(ExceptionMessage.USERNAME_TAKEN);
            }

            // Add account to database
            var newAccount = new Account
            {
                Username = signUpDTO.Username,
                Email = signUpDTO.Email,
                Password = CustomHash.GetHashString(signUpDTO.Password),
            };
            await _mongoService.Accounts.InsertOneAsync(newAccount);

            // TODO: Send confirmation email
            // This should be push into a message queue to be processed by background job

            return true;
        }

        public async Task<string> GoogleAuthentication(GoogleOAuthDTO googleOAuthDTO)
        {
            // Request Google account info from given access token
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", googleOAuthDTO.Token);

            var response = await httpClient.GetAsync("https://www.googleapis.com/userinfo/v2/me");
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Cannot connect to Google OAuth API");
            }

            var ggAcountInfo = await response.Content.ReadFromJsonAsync<GoogleOAuthAPIResponse>()
                ?? throw new Exception("Cannot parse Google account info with selected format");

            // Find existing account associated with fetched account's googleId
            var foundAccount = _mongoService.Accounts
                .Find(acc => acc.GoogleId == ggAcountInfo.GoogleId)
                .FirstOrDefault();

            // Create new account if none found
            if (foundAccount == null)
            {
                var newAccount = new Account
                {
                    Username = ggAcountInfo.Name,
                    Email = ggAcountInfo.Email,
                    Verified = true,
                    GoogleId = ggAcountInfo.GoogleId,
                    // This can be changed later if user want to
                    Password = Guid.NewGuid().ToString()
                };
                try
                {
                    await _mongoService.Accounts.InsertOneAsync(newAccount);
                }
                catch
                {
                    throw new Exception(ExceptionMessage.SERVER_UNAVAILABLE);
                }

                return newAccount.Id;
            }

            return foundAccount.Id;
        }

        // Check if email exists
        private async Task<bool> CheckEmailExist(string email)
        {
            var foundAccount = await _mongoService.Accounts
                .Find(
                    acc => acc.Email == email
                )
                .FirstOrDefaultAsync();
            return foundAccount != null;
        }

        //Check if username exists
        private async Task<bool> CheckUsernameExist(string? username)
        {
            var foundAccount = await _mongoService.Accounts
                .Find(
                    acc => acc.Username == username
                )
                .FirstOrDefaultAsync();
            return foundAccount != null;
        }
    }

    internal class GoogleOAuthAPIResponse
    {
        [JsonPropertyName("id")]
        public string GoogleId { get; set; }

        public string Email { get; set; }

        public string Name { get; set; }
    }
}

