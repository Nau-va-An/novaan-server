using System;
namespace NovaanServer.src.ExceptionLayer.CustomExceptions
{
    public static class ErrorCodes
    {
        // Common
        public const int SERVER_UNAVAILABLE = 1000;
        public const int DATABASE_UNAVAILABLE = 1001;
        public const int S3_UNAVAILABLE = 1002;
        public const int GG_UNAVAILABLE = 1003;

        // Authentication
        public const int EMAIL_TAKEN_BASIC = 1004;
        public const int EMAIL_TAKEN_GG = 1005;
        public const int EMAIL_OR_PASSWORD_NOT_FOUND = 1006;
        public const int RT_JWT_INVALID = 1007;
        public const int RT_JWT_UNAUTHORIZED = 1008;
    }
}

