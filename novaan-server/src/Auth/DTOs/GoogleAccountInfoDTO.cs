using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace NovaanServer.src.Auth.DTOs
{
    public class GoogleAccountInfoDTO
    {
        [JsonPropertyName("id")]
        public string GoogleId { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

    }
}