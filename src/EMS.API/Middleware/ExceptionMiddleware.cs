using System.Net;
using System.Text.Json;
using EMS.Application.Common.DTOs;
using Microsoft.EntityFrameworkCore;

namespace EMS.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unhandled exception: {Message} | Path: {Path}",
                ex.Message,
                context.Request.Path);

            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(
        HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message) = ex switch
        {
            ArgumentNullException => (
                HttpStatusCode.BadRequest,
                "Required data is missing."),
            UnauthorizedAccessException => (
                HttpStatusCode.Unauthorized,
                "Unauthorized access."),
            KeyNotFoundException => (
                HttpStatusCode.NotFound,
                "Resource not found."),
            InvalidOperationException => (
                HttpStatusCode.BadRequest,
                "An invalid request was made."),
            DbUpdateException dbEx => (
                HttpStatusCode.BadRequest,
                dbEx.InnerException?.Message.Contains("duplicate") == true
                    ? "A record with this value already exists."
                    : "A database constraint was violated."),
            _ => (
                HttpStatusCode.InternalServerError,
                "An unexpected error occurred. Please try again.")
        };

        context.Response.StatusCode = (int)statusCode;

        var response = ApiResponse<string>.Fail(message);
        var json = JsonSerializer.Serialize(response,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

        await context.Response.WriteAsync(json);
    }
}