using System;
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

        public Task<bool> SignInWithCredentials(SignInDTOs signInDTO)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SignInGoogle()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> SignUpWithCredentials(SignUpDTO signUpDTO)
        {
            var emailExisted = await checkEmailExist(signUpDTO.Email);
            if (emailExisted)
            {
                throw new BadHttpRequestException(ExceptionMessage.EMAIL_TAKEN);
            }

            var usernameExisted = await checkUsernameExist(signUpDTO.Username);
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
            try
            {
                await _mongoService.Accounts.InsertOneAsync(newAccount);
            }
            catch
            {
                throw new Exception(ExceptionMessage.SERVER_UNAVAILABLE);
            }

            // TODO: Send confirmation email
            // This should be push into a message queue to be processed by background job

            return true;
        }

        public async Task<bool> GoogleAuthentication(GoogleOauthDTO googleOauthDTO)
        {
            // Check if google id exists
            var userGoogleId = await checkGoogleIdExist(googleOauthDTO.Sub);
            if (!userGoogleId)
            {
                // Add account to database
                var newAccount = new Account
                {
                    Username = googleOauthDTO.Name??"",
                    Email = googleOauthDTO.Email??"",
                    Verified = true,
                    GoogleId = googleOauthDTO.Sub,
                };
                try
                {
                    await _mongoService.Accounts.InsertOneAsync(newAccount);
                }
                catch
                {
                    throw new Exception(ExceptionMessage.SERVER_UNAVAILABLE);
                }
            }
            return true;
        }

        // Check if google id exists
        private async Task<bool> checkGoogleIdExist(string? googleId)
        {
            if (string.IsNullOrEmpty(googleId))
            {
                return false;
            }
            var foundAccount = await _mongoService.Accounts
                .Find(
                    acc => acc.GoogleId == googleId
                )
                .FirstOrDefaultAsync();
            return foundAccount != null;
        }

        // Check if email exists
        private async Task<bool> checkEmailExist(string email)
        {
            var foundAccount = await _mongoService.Accounts
                .Find(
                    acc => acc.Email == email
                )
                .FirstOrDefaultAsync();
            return foundAccount != null;
        }

        //Check if username exists
        private async Task<bool> checkUsernameExist(string? username)
        {
            var foundAccount = await _mongoService.Accounts
                .Find(
                    acc => acc.Username == username
                )
                .FirstOrDefaultAsync();
            return foundAccount != null;
        }
    }
}

