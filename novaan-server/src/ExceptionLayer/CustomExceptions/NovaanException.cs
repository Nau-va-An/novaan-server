using System;
using System.Net;

namespace NovaanServer.src.ExceptionLayer.CustomExceptions
{
    public class NovaanException : Exception
    {
        public HttpStatusCode StatusCode { get; }

        public int ErrCode { get; }

        public NovaanException(HttpStatusCode statusCode, int errCode): base()
        {
            StatusCode = statusCode;
            ErrCode = errCode;
        }

        public NovaanException(HttpStatusCode statusCode, int errCode, string message): base(message)
        {
            StatusCode = statusCode;
            ErrCode = errCode;
        }
    }
}

