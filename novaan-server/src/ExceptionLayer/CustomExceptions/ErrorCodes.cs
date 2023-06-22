using System;
namespace NovaanServer.src.ExceptionLayer.CustomExceptions
{
    public static class ErrorCodes
    {
        // TODO: Please update latest error code to avoid duplicate value
        // Latest error code: 1031

        // Common
        public const int SERVER_UNAVAILABLE = 1000;
        public const int DATABASE_UNAVAILABLE = 1001;
        public const int S3_UNAVAILABLE = 1002;
        public const int GG_UNAVAILABLE = 1003;

        public const int FIELD_REQUIRED = 1024;

        // Authentication
        public const int USER_NOT_FOUND = 1025;
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
        public const int CONTENT_STEP_TOO_MANY = 1020;
        public const int CONTENT_PREP_TIME_TOO_LONG = 1021;
        public const int CONTENT_COOK_TIME_TOO_LONG = 1022;
        public const int CONTENT_INGR_TOO_MANY = 1023;

        // Followership
        public static int USER_ALREADY_FOLLOWING = 1026;
        public static int FOLLOWERSHIP_NOT_CREATED = 1027;
        public static int USER_NOT_FOLLOWING = 1028;
        public static int FOLLOWERSHIP_NOT_DELETED = 1029;

        // Profile
        public const int PROFILE_USER_NOT_FOUND = 1030;
        public static int FORBIDDEN_PROFILE_CONTENT = 1031;

        public static Dictionary<int, string> ErrorNameDictionary = typeof(ErrorCodes).GetFields()
                .Where(f => f.FieldType == typeof(int))
                .ToDictionary(f => (int)f.GetValue(null), f => f.Name);

    }
}

