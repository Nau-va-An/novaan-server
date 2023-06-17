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
using NovaanServer.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Amazon.Runtime;

namespace NovaanServer.src.Auth.Jwt
{
    public class JwtService : IJwtService
    {
        private readonly MongoDBService _mongoDBService;
        private readonly JwtConfig _jwtConfig;

        public JwtService(IOptions<JwtConfig> options, MongoDBService mongoDBService)
        {
            _jwtConfig = options.Value;
            _mongoDBService = mongoDBService;
        }

        public async Task<string> GenerateJwtToken(UserJwt userToken)
        {
            var jwtTokenHanler = new JwtSecurityTokenHandler();

            var jwtId = Guid.NewGuid().ToString();
            var tokenDescriptor = getTokenDescriptor(jwtId, userToken.UserId, Encoding.UTF8.GetBytes(_jwtConfig.Secret));
            var jwtToken = jwtTokenHanler.CreateEncodedJwt(tokenDescriptor);
            if (jwtToken == null)
            {
                throw new Exception("Unable to generate new access token");
            }

            var foundRefreshToken = (await _mongoDBService.RefreshTokens
                .FindAsync(rt => rt.UserId == userToken.UserId))
                .FirstOrDefault();

            if (foundRefreshToken == null)
            {
                // First time login
                var refreshToken = new RefreshToken()
                {
                    CurrentJwtId = jwtId,
                    UserId = userToken.UserId,
                    ValidTokenFamily = new List<string>() { jwtToken },
                    RevokeTokenFamily = new List<string>(),
                    AddedDate = DateTime.UtcNow,
                    ExpiryDate = DateTime.UtcNow.AddMonths(6),
                    IsRevoked = false,
                };
                await _mongoDBService.RefreshTokens.InsertOneAsync(refreshToken);
            }
            else
            {
                // Next device onward
                await appendValidAccessToken(foundRefreshToken, jwtToken);
            }

            return jwtToken;
        }

        public async Task<string> RefreshToken(string accessToken)
        {
            try
            {
                var jwtTokenHanler = new JwtSecurityTokenHandler();
                var token = jwtTokenHanler.ReadJwtToken(accessToken);
                if (token == null)
                {
                    throw new UnauthorizedAccessException();
                }

                var userId = token.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.NameId);
                if (userId == null)
                {
                    throw new UnauthorizedAccessException();
                }

                // Find refresh token linked with payload userId
                var refreshToken = (await _mongoDBService.RefreshTokens
                    .FindAsync(rt => rt.UserId.Equals(userId.Value) && !rt.IsRevoked))
                    .FirstOrDefault();
                if (refreshToken == null)
                {
                    throw new UnauthorizedAccessException();
                }

                // Revoke refresh token if previously used or expired
                if (refreshToken.RevokeTokenFamily.Contains(accessToken) ||
                    refreshToken.ExpiryDate.CompareTo(DateTime.UtcNow) < 0)
                {
                    revokeRefreshToken(refreshToken);
                    throw new UnauthorizedAccessException();
                }

                var jwtId = Guid.NewGuid().ToString();
                var tokenDescriptor = getTokenDescriptor(jwtId, userId.Value, Encoding.UTF8.GetBytes(_jwtConfig.Secret));
                var newAccessToken = jwtTokenHanler.CreateEncodedJwt(tokenDescriptor);
                if (newAccessToken == null)
                {
                    throw new Exception("Unable to generate new access token");
                }

                await refreshAccessToken(refreshToken, accessToken);
                return newAccessToken;
            }
            catch (UnauthorizedAccessException unAuthEx)
            {
                if (string.IsNullOrEmpty(unAuthEx.Message))
                {
                    throw;
                }
                throw new UnauthorizedAccessException(ExceptionMessage.ACCESS_TOKEN_INVALID);
            }
        }

        private SecurityTokenDescriptor getTokenDescriptor(string jwtId, string userId, byte[] key)
        {
            var expires = DateTime.UtcNow.Add(_jwtConfig.JwtExp);
            return new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.NameId, userId),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                }),

                Expires = expires,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };
        }

        private void revokeRefreshToken(RefreshToken refreshToken)
        {
            var filter = Builders<RefreshToken>.Filter
                        .Eq(rt => rt.Id, refreshToken.Id);
            var update = Builders<RefreshToken>.Update
                .Set(rt => rt.IsRevoked, true);
            _mongoDBService.RefreshTokens.UpdateOne(filter, update);
        }

        private async Task refreshAccessToken(RefreshToken refreshToken, string oldToken)
        {
            var filter = Builders<RefreshToken>.Filter
                .Eq(rt => rt.Id, refreshToken.Id);
            var update = Builders<RefreshToken>.Update
                .Push(rt => rt.RevokeTokenFamily, oldToken);
            await _mongoDBService.RefreshTokens.UpdateOneAsync(filter, update);
        }

        private async Task appendValidAccessToken(RefreshToken refreshToken, string newToken)
        {
            var filter = Builders<RefreshToken>.Filter
                .Eq(rt => rt.Id, refreshToken.Id);
            var update = Builders<RefreshToken>.Update
                .Push(rt => rt.ValidTokenFamily, newToken);
            await _mongoDBService.RefreshTokens.UpdateOneAsync(filter, update);
        }
    }
}
