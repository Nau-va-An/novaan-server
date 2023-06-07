using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MongoConnector.Models;
using MongoConnector;
using NovaanServer.src.Auth.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MongoDB.Driver;

namespace NovaanServer.src.Auth.Jwt
{
    public class JwtService
    {
        private readonly MongoDBService _mongoDBService;
        private readonly IConfiguration _configuration;
        private readonly TokenValidationParameters _tokenValidationParameters;

        public JwtService(IConfiguration configuration, TokenValidationParameters tokenValidationParameters, MongoDBService mongoDBService)
        {
            _configuration = configuration;
            _tokenValidationParameters = tokenValidationParameters;
            _mongoDBService = mongoDBService;
        }
        public async Task<SignInResponseDTO> GennerateJwtToken(UserJwt user)
        {
            var jwtTokenHanler = new JwtSecurityTokenHandler();

            var key = Encoding.UTF8.GetBytes(_configuration.GetSection("JwTConfig:Secret").Value);

            // Token desciptor
            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("email", user.Email),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTime.Now.ToUniversalTime().ToString()),
                }),

                Expires = DateTime.UtcNow.Add(TimeSpan.Parse(_configuration.GetSection("JwTConfig:ExpiryTimeFrame").Value)),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };

            var token = jwtTokenHanler.CreateToken(tokenDescriptor);
            var jwtToken = jwtTokenHanler.WriteToken(token);

            var refreshToken = new RefreshToken()
            {
                JwtId = token.Id,
                Token = RandomStringGeneration(23), //Generate a refresh token
                AddedDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddMonths(6),
                IsRevoked = false,
                IsUsed = false,
                UserMail = user.Email
            };

            await _mongoDBService.RefreshTokens.InsertOneAsync(refreshToken);

            return new SignInResponseDTO()
            {
                Token = jwtToken,
                RefreshToken = refreshToken.Token,
                Result = true
            };
        }

        public async Task<SignInResponseDTO> VerifyAndGennerateToken(TokenRequest tokenRequest)
        {
            var jwtTokenHandle = new JwtSecurityTokenHandler();

            try
            {
                _tokenValidationParameters.ValidateLifetime = false; //for testing

                var tokenInVerification =
                    jwtTokenHandle.ValidateToken(tokenRequest.Token, _tokenValidationParameters, out var validedToken);

                if (validedToken is JwtSecurityToken jwtSecurityToken)
                {
                    var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                        StringComparison.InvariantCultureIgnoreCase);

                    if (result == null)
                        return null;
                }

                var utcExpiryDate = long.Parse(tokenInVerification.Claims
                    .FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);

                var expiryDate = UnixTimeStampToDateTime(utcExpiryDate);
                if (expiryDate > DateTime.Now)
                {
                    return new SignInResponseDTO()
                    {
                        Result = false,
                        Errors = new List<string>()
                        {
                            "Expired token"
                        }
                    };
                }

                RefreshToken storedToken = await _mongoDBService.RefreshTokens.Find(x => x.Token == tokenRequest.RefreshToken).FirstOrDefaultAsync();

                if (storedToken == null)
                    return new SignInResponseDTO()
                    {
                        Result = false,
                        Errors = new List<string>()
                        {
                            "Invalid tokens"
                        }
                    };

                if (storedToken.IsUsed)
                    return new SignInResponseDTO()
                    {
                        Result = false,
                        Errors = new List<string>()
                        {
                            "Invalid tokens"
                        }
                    };

                if (storedToken.IsRevoked)
                    return new SignInResponseDTO()
                    {
                        Result = false,
                        Errors = new List<string>()
                        {
                            "Invalid tokens"
                        }
                    };

                var jti = tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;
                if (storedToken.JwtId != jti)
                    return new SignInResponseDTO()
                    {
                        Result = false,
                        Errors = new List<string>()
                        {
                            "Invalid tokens"
                        }
                    };

                if (storedToken.ExpiryDate < DateTime.UtcNow)
                    return new SignInResponseDTO()
                    {
                        Result = false,
                        Errors = new List<string>()
                        {
                            "Expired tokens"
                        }
                    };

                var filterStoredToken = Builders<RefreshToken>.Filter.Eq("Token", tokenRequest.RefreshToken);
                var updateStoredToken = Builders<RefreshToken>.Update.Set("IsUsed", true);
                await _mongoDBService.RefreshTokens.UpdateOneAsync(filterStoredToken, updateStoredToken);


                return await GennerateJwtToken(new UserJwt()
                {
                    Email = storedToken.UserMail,
                });

            }
            catch (Exception e)
            {
                return new SignInResponseDTO()
                {
                    Result = false,
                    Errors = new List<string>()
                        {
                            "Server error"
                        }
                };
            }
        }

        private DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            var dateTimeVal = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTimeVal = dateTimeVal.AddSeconds(unixTimeStamp).ToUniversalTime();

            return dateTimeVal;
        }

        private string RandomStringGeneration(int length)
        {
            var random = new Random();
            var chars = "ABCDEFGHIJKMNOPqRSTUVWXYZ1234567890abcdefghijkmnopqrstuvwxyz_";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }


    }
}
