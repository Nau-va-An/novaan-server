using System;
using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NovaanServer.src.ExceptionLayer.CustomExceptions;

namespace NovaanServer.src.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class MultipartFormDataAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var request = context.HttpContext.Request;
            if (request.HasFormContentType &&
                request.ContentType != null &&
                request.ContentType.StartsWith(
                    "multipart/form-data",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return;
            }

            throw new NovaanException(
                ErrorCodes.CONTENT_CONTENT_TYPE_INVALID,
                HttpStatusCode.UnsupportedMediaType
            );
        }
    }
}

