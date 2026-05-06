using System.Net;
using BackendIntegrador.Api.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BackendIntegrador.Api.Filters;

public class ApiResponseFilter : IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is ObjectResult objectResult)
        {
            var statusCode = (HttpStatusCode)(objectResult.StatusCode ?? StatusCodes.Status200OK);

            var apiResponse = new ApiResponse<object>
            {
                success = statusCode is >= HttpStatusCode.OK and < HttpStatusCode.MultipleChoices,
                status = statusCode,
                method = context.HttpContext.Request.Method,
                errors = objectResult.Value is ValidationProblemDetails validationDetails 
                         ? string.Join("; ", validationDetails.Errors.Select(e => $"{e.Key}: {string.Join(", ", e.Value)}"))
                         : null,
                response = objectResult.Value is ValidationProblemDetails ? null : objectResult.Value
            };

            context.Result = new ObjectResult(apiResponse)
            {
                StatusCode = objectResult.StatusCode
            };
        }
        else if (context.Result is StatusCodeResult statusCodeResult)
        {
            var statusCode = (HttpStatusCode)statusCodeResult.StatusCode;
            var apiResponse = new ApiResponse<object>
            {
                success = statusCode is >= HttpStatusCode.OK and < HttpStatusCode.MultipleChoices,
                status = statusCode,
                method = context.HttpContext.Request.Method,
                response = null
            };

            context.Result = new ObjectResult(apiResponse)
            {
                StatusCode = statusCodeResult.StatusCode
            };
        }

        await next();
    }
}