using NovaanServer.src.Common.DTOs;

namespace NovaanServer.src.Auth.DTOs
{
    public class SignInResDTO : BaseResponse
    {
        public string Token { get; set; }
    }
}
