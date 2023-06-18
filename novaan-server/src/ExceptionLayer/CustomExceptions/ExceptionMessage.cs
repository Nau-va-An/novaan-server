using System;
namespace NovaanServer.src.ExceptionLayer.CustomExceptions
{
    public static class ExceptionMessage
    {
        public const string EMAIL_TAKEN = "This email had been associated with another account.";

        public const string USERNAME_TAKEN = "This username had been associated with another account.";

        public const string SERVER_UNAVAILABLE = "Server is currently unavailable. Please try again later.";

        public const string EMAIL_OR_PASSWORD_NOT_FOUND = "Wrong email/username and password combination.";

        public const string ACCESS_TOKEN_INVALID = "Access token is not valid. Unauthorized.";

        public const string DATABASE_UNAVAILABLE = "Database is currently unavailable. Please try again later.";

        public const string S3_UNAVAILABLE = "S3 Bucket is currently unavailable. Please try again later.";
    }
}

