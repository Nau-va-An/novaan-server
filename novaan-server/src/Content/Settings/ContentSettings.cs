using System;
namespace NovaanServer.src.Content.Settings
{
    public static class ContentSettings
    {
        public readonly static TimeSpan COOK_MAX = TimeSpan.Parse("3.00:59:00"); // 72 hours + 59 minutes
        public readonly static TimeSpan PREP_MAX = TimeSpan.Parse("3.00:59:00"); // 72 hours + 50 minutes

        public const long VideoSizeLimit = 20L * 1024L * 1024L; // 20MB
        public const long ImageSizeLimit = 5L * 1024L * 1024L; // 5MB

        public readonly static string[] PermittedVidExtension = { ".mp4" };
        public readonly static string[] PermittedImgExtension = { ".jpg", ".jpeg", ".png", ".webp" };
    }
}

