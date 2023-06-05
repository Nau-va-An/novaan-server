using System;
using MongoConnector;
using MongoConnector.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using NovaanServer.Auth.DTOs;
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
            // Check if email exist
            var foundAccount = _mongoService.Accounts
                .Find(
                    acc => acc.Email == signUpDTO.Email && acc.Verified
                )
                .FirstOrDefault();
            if (foundAccount != null)
            {
                throw new BadHttpRequestException(ExceptionMessage.EMAIL_TAKEN);
            }

            // Check if username exist
            foundAccount = _mongoService.Accounts
                .Find(
                    acc => acc.Username == signUpDTO.Username
                )
                .FirstOrDefault();
            if (foundAccount != null)
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

        public Task<bool> SignUpGoogle()
        {
            throw new NotImplementedException();
        }
    }
}

