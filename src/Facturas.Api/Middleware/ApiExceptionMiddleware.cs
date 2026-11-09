using Facturas.Application.Common.Exceptions;
using Facturas.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Facturas.Api.Middleware;

public sealed class ApiExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiExceptionMiddleware> _logger;

    public ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger)
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
        catch (NotFoundException exception)
        {
            await WriteProblemAsync(context, StatusCodes.Status404NotFound, "Recurso no encontrado", exception.Message);
        }
        catch (ValidationException exception)
        {
            await WriteProblemAsync(context, StatusCodes.Status400BadRequest, "Solicitud inválida", exception.Message);
        }
        catch (ConflictException exception)
        {
            await WriteProblemAsync(context, StatusCodes.Status409Conflict, "Conflicto de negocio", exception.Message);
        }
        catch (DomainException exception)
        {
            await WriteProblemAsync(context, StatusCodes.Status409Conflict, "Regla de negocio", exception.Message);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error no controlado procesando {Method} {Path}.", context.Request.Method, context.Request.Path);
            await WriteProblemAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "Error interno",
                "Ocurrió un error inesperado. Revisa los logs del servidor.");
        }
    }

    private static async Task WriteProblemAsync(HttpContext context, int statusCode, string title, string detail)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        });
    }
}
