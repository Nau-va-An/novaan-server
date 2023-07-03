using System.Net;
using System.Net.Http.Headers;
using MongoConnector;
using MongoConnector.Models;
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
                 .FindAsync(acc => acc.Email == signInDTO.Email))
                 .FirstOrDefault()
                 ?? throw new NovaanException(ErrorCodes.EMAIL_OR_PASSWORD_NOT_FOUND, HttpStatusCode.BadRequest);

            var hashPassword = CustomHash.GetHashString(signInDTO.Password);
            if (foundUser.Password != hashPassword)
            {
                throw new NovaanException(
                    ErrorCodes.EMAIL_OR_PASSWORD_NOT_FOUND,
                    HttpStatusCode.BadRequest
                );
            }

            return foundUser.UserId;
        }

        public async Task<bool> SignUpWithCredentials(SignUpDTO signUpDTO)
        {
            var emailExisted = await CheckEmailExist(signUpDTO.Email);
            if (emailExisted)
            {
                throw new NovaanException(ErrorCodes.EMAIL_TAKEN_BASIC, HttpStatusCode.BadRequest);
            }

            // Add account + user to database
            try
            {
                var password = CustomHash.GetHashString(signUpDTO.Password);
                var userId = await CreateNewUser(signUpDTO.DisplayName);
                await CreateNewAccount(signUpDTO.Email, userId, password, null);
            }
            catch
            {
                throw new NovaanException(ErrorCodes.DATABASE_UNAVAILABLE);
            }

            // TODO: Send confirmation email
            // This should be push into a message queue to be processed by background job

            return true;
        }

        public async Task<string> GoogleAuthentication(GoogleOAuthDTO googleOAuthDTO)
        {
            var ggAcountInfo = await GetGoogleAccountInfo(googleOAuthDTO);

            // Find existing account associated with fetched account's googleId
            var foundAccount = (await _mongoService.Accounts
                .FindAsync(acc => acc.GoogleId == ggAcountInfo.GoogleId))
                .FirstOrDefault();

            // Create new account if none found
            if (foundAccount == null)
            {
                // Reject request if email existed
                var emailExisted = await CheckEmailExist(ggAcountInfo.Email);
                if (emailExisted)
                {
                    throw new NovaanException(ErrorCodes.EMAIL_TAKEN_BASIC, HttpStatusCode.BadRequest);
                }

                try
                {
                    var userId = await CreateNewUser(ggAcountInfo.Name);
                    await CreateNewAccount(ggAcountInfo.Email, userId, null, ggAcountInfo.GoogleId);

                    return userId;
                }
                catch
                {
                    throw new NovaanException(ErrorCodes.DATABASE_UNAVAILABLE);
                }
            }

            return foundAccount.Id;
        }

        // Check if email exists
        private async Task<bool> CheckEmailExist(string email)
        {
            // Check if email existed in account collection
            Account foundAccount = (await _mongoService.Accounts
                .FindAsync(acc => acc.Email == email))
                .FirstOrDefault();
            return foundAccount != null;
        }

        private static async Task<GoogleAccountInfoDTO> GetGoogleAccountInfo(GoogleOAuthDTO googleOAuthDTO)
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
                throw new NovaanException(ErrorCodes.GG_UNAVAILABLE);
            }

            var ggAcountInfo = await response.Content.ReadFromJsonAsync<GoogleAccountInfoDTO>()
                ?? throw new Exception("Cannot parse Google account info with selected format");

            return ggAcountInfo;
        }

        private async Task<string> CreateNewAccount(string email, string userId, string? password, string? googleId = null)
        {

            var newAccount = new Account
            {
                Email = email,
                Verified = googleId == null ? true : false,
                UserId = userId,
                GoogleId = googleId,
                // Generate random password when people sign up with Google account
                Password = password ?? Guid.NewGuid().ToString(),
            };
            await _mongoService.Accounts.InsertOneAsync(newAccount);

            return newAccount.Id;
        }

        private async Task<string> CreateNewUser(string displayName)
        {
            var newUser = new User
            {
                DisplayName = displayName,
            };

            await _mongoService.Users.InsertOneAsync(newUser);

            return newUser.Id;
        }
    }
}

