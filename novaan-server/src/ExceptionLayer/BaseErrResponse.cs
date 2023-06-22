using System;
using System.Text.Json.Serialization;

namespace NovaanServer.ExceptionLayer.CustomExceptions
{
    public class BaseErrResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("errCode")]
        public int ErrCode { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }
}

