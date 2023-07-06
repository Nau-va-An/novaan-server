using NovaanServer.src.ExceptionLayer.CustomExceptions;
using System.IdentityModel.Tokens.Jwt;
using System.Net;

namespace NovaanServer.src.Common.Utils
{
    public static class CustomHeader
    {
        public static JwtSecurityToken? GetBearerToken(this HttpRequest request)
        {
            var authorizations = request.Headers["Authorization"];
            if (authorizations.Count == 0)
            {
                return null;
            }

            // There should be one and only one access token in one request
            var authHeader = authorizations[0];
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                var token = authHeader["Bearer ".Length..];
                var tokenHandler = new JwtSecurityTokenHandler();
                return tokenHandler.ReadJwtToken(token);
            }

            return null;
        }

        public static string? GetUserId(this HttpRequest request)
        {
            var token = request.GetBearerToken();
            if (token == null)
            {
                return null;
            }

            var userId = token.Claims.FirstOrDefault(cl => cl.Type == JwtRegisteredClaimNames.NameId);
            return userId == null || userId.Value == null
                ? throw new NovaanException(ErrorCodes.USER_NOT_FOUND, HttpStatusCode.NotFound)
                : userId.Value;
        }
    }
}

