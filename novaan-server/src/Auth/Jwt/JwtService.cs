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
    public class JwtService: IJwtService
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
            var tokenDescriptor = getTokenDescriptor(jwtId, Encoding.UTF8.GetBytes(_jwtConfig.Secret));
            var jwtToken = jwtTokenHanler.CreateEncodedJwt(tokenDescriptor);
            if(jwtToken == null)
            {
                throw new Exception("Unable to generate new access token");
            }

            var refreshToken = new RefreshToken()
            {
                CurrentJwtId = jwtId,
                UserId = userToken.UserId,
                TokenFamily = new List<string>() { jwtToken },
                AddedDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddMonths(6),
                IsRevoked = false,
            };

            await _mongoDBService.RefreshTokens.InsertOneAsync(refreshToken);

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

                var userId = token.Claims.FirstOrDefault(c => c.ValueType == JwtRegisteredClaimNames.NameId);
                if (userId == null)
                {
                    throw new UnauthorizedAccessException();
                }

                // Find refresh token linked with payload userId
                var refreshToken = (await _mongoDBService.RefreshTokens
                    .FindAsync(rt => rt.UserId == userId.Value && !rt.IsRevoked))
                    .FirstOrDefault();
                if (refreshToken == null)
                {
                    throw new UnauthorizedAccessException();
                }

                // Revoke refresh token if previously used or does not match with current jwtId or expired
                if (refreshToken.TokenFamily.Contains(accessToken) ||
                    refreshToken.CurrentJwtId != token.Id ||
                    refreshToken.ExpiryDate.CompareTo(DateTime.UtcNow) > 0)
                {
                    revokeRefreshToken(refreshToken);
                    throw new UnauthorizedAccessException();
                }

                var jwtId = Guid.NewGuid().ToString();
                var tokenDescriptor = getTokenDescriptor(jwtId, Encoding.UTF8.GetBytes(_jwtConfig.Secret));
                var newAccessToken = jwtTokenHanler.CreateEncodedJwt(tokenDescriptor);
                if (newAccessToken == null)
                {
                    throw new Exception("Unable to generate new access token");
                }

                await refreshAccessToken(refreshToken, jwtId, accessToken);
                return newAccessToken;
            }
            catch (UnauthorizedAccessException unAuthEx)
            {
                if (string.IsNullOrEmpty(unAuthEx.Message))
                {
                    throw new UnauthorizedAccessException(unAuthEx.Message);
                }
                throw new UnauthorizedAccessException("Access token is not valid. Unauthorized");
            }
        }

        private SecurityTokenDescriptor getTokenDescriptor(string jwtId, byte[] key)
        {
            var expires = DateTime.UtcNow.Add(_jwtConfig.JwtExp);
            return new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Jti, jwtId),
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

        private async Task refreshAccessToken(RefreshToken refreshToken, string jwtId, string oldToken)
        {
            var filter = Builders<RefreshToken>.Filter
                .Eq(rt => rt.Id, refreshToken.Id);
            var update = Builders<RefreshToken>.Update
                .Set(rt => rt.CurrentJwtId, jwtId)
                .Push(rt => rt.TokenFamily, oldToken);
            await _mongoDBService.RefreshTokens.UpdateOneAsync(filter, update);
        }
    }
}
