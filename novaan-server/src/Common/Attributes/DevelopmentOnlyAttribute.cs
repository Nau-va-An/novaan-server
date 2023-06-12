using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace NovaanServer.src.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class DevelopmentOnlyAttribute: Attribute, IResourceFilter
	{
        public void OnResourceExecuted(ResourceExecutedContext context)
        {
            // Do nothing
        }

        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            // Only allow access when on development mode
            var env = context.HttpContext.RequestServices.GetService<IWebHostEnvironment>();
            if(!env.IsDevelopment())
            {
                context.Result = new NotFoundResult();
            }
        }
    }
}

