using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MongoConnector.Models;
using MongoConnector;
using NovaanServer.src.Auth.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MongoDB.Driver;
using NovaanServer.src.ExceptionLayer.CustomExceptions;

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
        public async Task<SignInResponseDTO> GenerateJwtToken(UserJwt userToken)
        {
            var jwtTokenHanler = new JwtSecurityTokenHandler();

            var key = Encoding.UTF8.GetBytes(_configuration.GetSection("JwTConfig:Secret").Value);
            if (key == null)
            {
                throw new Exception("Cannot read JwTConfig settings in appsettings");
            }

            // Token desciptor
            var expires = DateTime.UtcNow.Add(TimeSpan.Parse(_configuration.GetSection("JwTConfig:ExpiryTimeFrame").Value));
            if (expires == null)
            {
                throw new Exception("Cannot read JwTConfig settings in appsettings");
            }

            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Email, userToken.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTime.Now.ToUniversalTime().ToString()),
                }),

                Expires = expires,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };

            var token = jwtTokenHanler.CreateToken(tokenDescriptor);
            var jwtToken = jwtTokenHanler.WriteToken(token);

            var refreshToken = new RefreshToken()
            {
                JwtId = token.Id,
                Token = randomStringGeneration(23), // Generate a refresh token
                AddedDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddMonths(6),
                IsRevoked = false,
                IsUsed = false,
                UserMail = userToken.Email
            };

            await _mongoDBService.RefreshTokens.InsertOneAsync(refreshToken);

            return new SignInResponseDTO()
            {
                Token = jwtToken,
                RefreshToken = refreshToken.Token,
                Result = true
            };
        }

        public async Task<SignInResponseDTO> VerifyAndGenerationToken(TokenRequest tokenRequest)
        {
            var jwtTokenHandle = new JwtSecurityTokenHandler();

            try
            {
                var tokenInVerification =
                    jwtTokenHandle.ValidateToken(tokenRequest.Token, _tokenValidationParameters, out var validedToken);

                if (validedToken is JwtSecurityToken jwtSecurityToken)
                {
                    var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);

                    if (result == null)
                    {
                        return null;
                    }
                }

                var utcExpiryDate = long.Parse(tokenInVerification.Claims
                    .FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);

                var expiryDate = unixTimeStampToDateTime(utcExpiryDate);
                if (expiryDate > DateTime.Now)
                {
                    throw new BadHttpRequestException(ExceptionMessage.TOKEN_EXPIRED);
                }

                RefreshToken storedToken = await _mongoDBService.RefreshTokens.Find(x => x.Token == tokenRequest.RefreshToken).FirstOrDefaultAsync();

                if (storedToken == null)
                {
                    throw new BadHttpRequestException(ExceptionMessage.TOKEN_INVALID);
                }

                if (storedToken.IsUsed)
                {
                    throw new BadHttpRequestException(ExceptionMessage.TOKEN_INVALID);
                }

                if (storedToken.IsRevoked)
                {
                    throw new BadHttpRequestException(ExceptionMessage.TOKEN_INVALID);
                }

                var jti = tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;
                if (storedToken.JwtId != jti)
                {
                    throw new BadHttpRequestException(ExceptionMessage.TOKEN_INVALID);
                }

                if (storedToken.ExpiryDate < DateTime.UtcNow)
                {
                    throw new BadHttpRequestException(ExceptionMessage.TOKEN_EXPIRED);
                }

                var filterStoredToken = Builders<RefreshToken>.Filter.Eq("Token", tokenRequest.RefreshToken);
                var updateStoredToken = Builders<RefreshToken>.Update.Set("IsUsed", true);
                await _mongoDBService.RefreshTokens.UpdateOneAsync(filterStoredToken, updateStoredToken);

                return await GenerateJwtToken(new UserJwt()
                {
                    Email = storedToken.UserMail,
                });

            }
            catch (Exception e)
            {
                throw new BadHttpRequestException(ExceptionMessage.SERVER_ERROR);
            }
        }

        private DateTime unixTimeStampToDateTime(long unixTimeStamp)
        {
            var dateTimeVal = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTimeVal = dateTimeVal.AddSeconds(unixTimeStamp).ToUniversalTime();

            return dateTimeVal;
        }

        private string randomStringGeneration(int length)
        {
            var random = new Random();
            var chars = "ABCDEFGHIJKMNOPqRSTUVWXYZ1234567890abcdefghijkmnopqrstuvwxyz_";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }


    }
}
