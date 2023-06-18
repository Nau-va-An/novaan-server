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
using System.Net;

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
            var jwtTokenHanler = new JwtSecurityTokenHandler();
            var token = jwtTokenHanler.ReadJwtToken(accessToken);
            if (token == null)
            {
                throw new NovaanException(HttpStatusCode.BadRequest, ErrorCodes.RT_JWT_INVALID);
            }

            var userId = token.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.NameId);
            if (userId == null)
            {
                throw new NovaanException(HttpStatusCode.BadRequest, ErrorCodes.RT_JWT_INVALID);
            }

            // Find refresh token linked with payload userId
            var refreshToken = (await _mongoDBService.RefreshTokens
                .FindAsync(rt => rt.UserId.Equals(userId.Value) && !rt.IsRevoked))
                .FirstOrDefault();
            if (refreshToken == null)
            {
                throw new NovaanException(HttpStatusCode.Unauthorized, ErrorCodes.RT_JWT_UNAUTHORIZED);
            }

            // Revoke refresh token if previously used or expired
            if (refreshToken.RevokeTokenFamily.Contains(accessToken) ||
                refreshToken.ExpiryDate.CompareTo(DateTime.UtcNow) < 0)
            {
                revokeRefreshToken(refreshToken);
                throw new NovaanException(HttpStatusCode.Unauthorized, ErrorCodes.RT_JWT_UNAUTHORIZED);
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
            try
            {
                _mongoDBService.RefreshTokens.UpdateOne(filter, update);
            } catch
            {
                throw new NovaanException(HttpStatusCode.InternalServerError, ErrorCodes.DATABASE_UNAVAILABLE);
            }
        }

        private async Task refreshAccessToken(RefreshToken refreshToken, string oldToken)
        {
            var filter = Builders<RefreshToken>.Filter
                .Eq(rt => rt.Id, refreshToken.Id);
            var update = Builders<RefreshToken>.Update
                .Push(rt => rt.RevokeTokenFamily, oldToken);
            try
            {
                await _mongoDBService.RefreshTokens.UpdateOneAsync(filter, update);
            }
            catch
            {
                throw new NovaanException(HttpStatusCode.InternalServerError, ErrorCodes.DATABASE_UNAVAILABLE);
            }
        }

        private async Task appendValidAccessToken(RefreshToken refreshToken, string newToken)
        {
            var filter = Builders<RefreshToken>.Filter
                .Eq(rt => rt.Id, refreshToken.Id);
            var update = Builders<RefreshToken>.Update
                .Push(rt => rt.ValidTokenFamily, newToken);
            try
            {
                await _mongoDBService.RefreshTokens.UpdateOneAsync(filter, update);

            }
            catch
            {
                throw new NovaanException(HttpStatusCode.InternalServerError, ErrorCodes.DATABASE_UNAVAILABLE);
            }
        }
    }
}
