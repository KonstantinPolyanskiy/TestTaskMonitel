using Main.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Main.WebApi.Middlewares;

/// <summary>
/// Middleware для корректного отображения ошибок и кодов.
/// </summary>
public sealed class ExceptionMappingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMappingMiddleware> _log;

    public ExceptionMappingMiddleware(RequestDelegate next, ILogger<ExceptionMappingMiddleware> log)
    {
        _next = next;
        _log = log;
    }

    public async Task Invoke(HttpContext ctx)
    {
        try
        {
            await _next(ctx);
        }
        catch (DomainException dex)
        {
            if (!ctx.Response.HasStarted)
            {
                ctx.Response.Clear();
                ctx.Response.StatusCode = dex.StatusCode;
                ctx.Response.ContentType = "application/problem+json";

                var problem = new ProblemDetails
                {
                    Status = dex.StatusCode,
                    Title = "Ошибка бизнес логики",
                    Detail = dex.Message
                };

                await ctx.Response.WriteAsJsonAsync(problem);
            }
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Unhandled exception.");

            if (!ctx.Response.HasStarted)
            {
                ctx.Response.Clear();
                ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
            }
        }
    }
}