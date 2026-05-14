using System.Net;
using System.Text.Json;
using BackendIntegrador.Api.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BackendIntegrador.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = exception switch
        {
            ArgumentException or ArgumentNullException or InvalidOperationException => HttpStatusCode.BadRequest,
            KeyNotFoundException => HttpStatusCode.NotFound,
            Microsoft.EntityFrameworkCore.DbUpdateException dbEx when dbEx.InnerException?.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase) == true 
                                                                     || dbEx.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true 
                                                                     || dbEx.InnerException?.Message.Contains("Violation of UNIQUE KEY constraint") == true => HttpStatusCode.BadRequest,
            _ => HttpStatusCode.InternalServerError
        };

        var errorMessage = statusCode == HttpStatusCode.BadRequest && exception is Microsoft.EntityFrameworkCore.DbUpdateException
            ? "El registro ya existe o viola una restricción de unicidad."
            : exception.Message;

        var response = new ApiResponse<object>
        {
            success = false,
            status = statusCode,
            method = context.Request.Method,
            errors = errorMessage,
            response = null
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}