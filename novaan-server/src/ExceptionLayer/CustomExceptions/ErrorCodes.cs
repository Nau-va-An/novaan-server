using System;
namespace NovaanServer.src.ExceptionLayer.CustomExceptions
{
    public static class ErrorCodes
    {
        // TODO: Please update latest error code to avoid duplicate value
        // Latest error code: 1019

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

        // Admin
        public const int ADMIN_SUBMISSION_NOT_FOUND = 1009;

        // Content
        public const int CONTENT_CONTENT_TYPE_INVALID = 1010;
        public const int CONTENT_DISPOSITION_INVALID = 1011;
        public const int CONTENT_FILENAME_INVALID = 1012;
        public const int CONTENT_FIELD_INVALID = 1013;
        public const int CONTENT_EXT_INVALID = 1014;
        public const int CONTENT_IMG_SIZE_INVALID = 1015;
        public const int CONTENT_IMG_RESO_INVALID = 1016;
        public const int CONTENT_VID_SIZE_INVALID = 1017;
        public const int CONTENT_VID_RESO_INVALID = 1018;
        public const int CONTENT_VID_LEN_INVALID = 1019;
    }
}

