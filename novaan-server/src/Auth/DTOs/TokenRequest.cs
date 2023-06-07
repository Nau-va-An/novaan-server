using System.ComponentModel.DataAnnotations;

namespace NovaanServer.src.Auth.DTOs
{
    public class TokenRequest
    {
        [Required]
        public string Token { get; set; }
        [Required]
        public string RefreshToken { get; set; }
    }
}
