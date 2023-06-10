namespace NovaanServer.Configuration
{
    public class JwtConfig
    {
        public string Secret { get; set; }

        public TimeSpan JwtExp { get; set; }

        public TimeSpan RefreshTokenExp { get; set; }
    }
}
