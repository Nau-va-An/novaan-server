using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
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

        // Sign in and return userId
        public async Task<string> SignInWithCredentials(SignInDTOs signInDTO)
        {
            var foundUser = (await _mongoService.Accounts
                 .FindAsync(acc => acc.Email == signInDTO.UsernameOrEmail))
                 .FirstOrDefault();
            if (foundUser == null)
            {
                throw new BadHttpRequestException(ExceptionMessage.EMAIL_OR_PASSWORD_NOT_FOUND);
            }

            var hashPassword = CustomHash.GetHashString(signInDTO.Password);
            if (foundUser.Password != hashPassword)
            {
                throw new BadHttpRequestException(ExceptionMessage.EMAIL_OR_PASSWORD_NOT_FOUND);
            }

            return foundUser.Id;
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
            await _mongoService.Accounts.InsertOneAsync(newAccount);

            // TODO: Send confirmation email
            // This should be push into a message queue to be processed by background job

            return true;
        }

        public async Task<bool> GoogleAuthentication(SignUpDTO signUpDTO)
        {
            // Check if user email exists
            // Check if user email exists
            var emailExists = await checkEmailExist(signUpDTO.Email);
            if (!emailExists)
            {
                // Add account to database
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
        private async Task<bool> checkUsernameExist(string username)
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

