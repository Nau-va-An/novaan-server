using System;
using System.Net;

namespace NovaanServer.src.ExceptionLayer.CustomExceptions
{
    public class NovaanException : Exception
    {
        public HttpStatusCode StatusCode { get; }

        public int ErrCode { get; }

        public NovaanException(int errCode, HttpStatusCode statusCode = HttpStatusCode.InternalServerError): base()
        {
            StatusCode = statusCode;
            ErrCode = errCode;
        }

        public NovaanException(int errCode, HttpStatusCode statusCode, string message): base(message)
        {
            StatusCode = statusCode;
            ErrCode = errCode;
        }
    }
}

