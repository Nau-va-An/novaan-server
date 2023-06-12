using System;
using NovaanServer.src.Common.DTOs;

namespace NovaanServer.src.Auth.DTOs
{
	public class RefreshTokenResDTO: BaseResponse
	{
        public string Token { get; set; }
    }
}

