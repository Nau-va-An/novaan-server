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
            if (CheckEmailExist(signUpDTO.Email).Result)
            {
                throw new BadHttpRequestException(ExceptionMessage.EMAIL_TAKEN);
            }

            if (CheckUsernameExist(signUpDTO.Username).Result)
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

        public async Task<bool> GoogleAuthentication(SignUpDTO signUpDTO)
        {
            //check if user email exists
            if (!CheckEmailExist(signUpDTO.Email).Result)
            {
                //add account to database
                var newAccount = new Account
                {
                    Username = signUpDTO.Username,
                    Email = signUpDTO.Email,
                    Verified = true,
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

        //check if email exists
        public async Task<bool> CheckEmailExist(string email)
        {
            var foundAccount = await _mongoService.Accounts
                .Find(
                    acc => acc.Email == email
                )
                .FirstOrDefaultAsync();
            if (foundAccount != null)
            {
                return true;
            }
            return false;
        }

        //check if username exists
        public async Task<bool> CheckUsernameExist(string username)
        {
            var foundAccount = await _mongoService.Accounts
                .Find(
                    acc => acc.Username == username
                )
                .FirstOrDefaultAsync();
            if (foundAccount != null)
            {
                return true;
            }
            return false;
        }
    }
}

