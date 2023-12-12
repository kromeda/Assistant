using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace Assistant.Web;

/// <summary>
/// Промежуточное ПО для обработки исключений, возникающих в конвейере выполнения HTTP запроса.
/// При возникновении исключения формирует ответ в виде сериализованного в json объекта типа <see cref="ProblemDetails"/>.
/// </summary>
internal sealed class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(ILogger<ExceptionMiddleware> logger, RequestDelegate next)
    {
        _logger = logger;
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Ошибка валидации, сообщение: {Message}", ex.Message);
            var problemDetails = CreateProblemDetails(ex);
            await WriteResponse(context, problemDetails, StatusCodes.Status400BadRequest);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ErrorTexts.TaskCanceled);
            var problemDetails = CreateProblemDetails(ex);
            await WriteResponse(context, problemDetails, StatusCodes.Status408RequestTimeout);
        }
        catch (ProblemDetailsException ex) when 
            (ex.ProblemDetails.Status is >= StatusCodes.Status400BadRequest and < StatusCodes.Status500InternalServerError)
        {
            
        }
        catch (ProblemDetailsException ex) when 
            (ex.ProblemDetails.Status >= StatusCodes.Status500InternalServerError)
        {

        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, ErrorTexts.RelatedSerivce);
            var problemDetails = CreateProblemDetails(ex);
            await WriteResponse(context, problemDetails, StatusCodes.Status502BadGateway);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ErrorTexts.Internal);
            var problemDetails = CreateProblemDetails(ex);
            await WriteResponse(context, problemDetails, StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task WriteResponse(HttpContext context, ProblemDetails problemDetails, int statusCode)
    {
        context.Response.ContentType = ContentTypes.ProblemJsonFull;
        context.Response.StatusCode = statusCode;

        await Json.Serialize(context.Response.Body, problemDetails, context.Response.HttpContext.RequestAborted);
    }

    private static ProblemDetails CreateProblemDetails(Exception exception)
    {
        return exception switch
        {
            ValidationException vx => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Status = StatusCodes.Status400BadRequest,
                Title = vx.Message,
                Detail = vx.ValidationResult?.ErrorMessage
            },
            TaskCanceledException => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.7",
                Status = StatusCodes.Status408RequestTimeout,
                Title = ErrorTexts.TaskCanceled
            },
            HttpRequestException => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.3",
                Status = StatusCodes.Status502BadGateway,
                Title = ErrorTexts.RelatedSerivce
            },
            Exception => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                Status = StatusCodes.Status500InternalServerError,
                Title = ErrorTexts.Internal
            }
        };
    }
}