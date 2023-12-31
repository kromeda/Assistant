﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Assistant.Web
{
    /// <summary>
    /// Промежуточное ПО для обработки исключений, возникающих в конвейере выполнения HTTP запроса.
    /// При возникновении исключения формирует ответ в виде сериализованного в json объекта типа <see cref="ProblemDetails"/>.
    /// </summary>
    internal sealed class ExceptionMiddleware
    {
        private static readonly ProblemDetailTypes _problemDetailTypes = new ProblemDetailTypes();
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
                _logger.LogWarning(ex, ErrorTexts.Validation);
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
                (ex.ProblemDetails.Status >= StatusCodes.Status400BadRequest &&
                ex.ProblemDetails.Status < StatusCodes.Status500InternalServerError)
            {
                LogProblemDetails(ex, LogLevel.Warning);
                await WriteResponse(context, ex.ProblemDetails, StatusCodes.Status400BadRequest);
            }
            catch (ProblemDetailsException ex) when
                (ex.ProblemDetails.Status >= StatusCodes.Status500InternalServerError)
            {
                LogProblemDetails(ex, LogLevel.Error);
                await WriteResponse(context, ex.ProblemDetails, StatusCodes.Status502BadGateway);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, ErrorTexts.RelatedSerivce);
                var problemDetails = CreateProblemDetails(ex);
                await WriteResponse(context, problemDetails, StatusCodes.Status502BadGateway);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ErrorTexts.Internal + " Тип ошибки: {ExceptionType}.", ex.GetType());
                var problemDetails = CreateProblemDetails(ex);
                await WriteResponse(context, problemDetails, StatusCodes.Status500InternalServerError);
            }
        }

        private void LogProblemDetails(ProblemDetailsException ex, LogLevel severity)
        {
            if (ex.ProblemDetails != null)
            {
                var templateBuilder = new StringBuilder();
                var args = new List<object>();

                if (!string.IsNullOrWhiteSpace(ex.ProblemDetails.Title))
                {
                    templateBuilder.Append(" Сообщение: {Message}.");
                    args.Add(ex.ProblemDetails.Title.Trim('.'));
                }

                if (!string.IsNullOrWhiteSpace(ex.ProblemDetails.Detail))
                {
                    templateBuilder.Append(" Описание: {Detail}.");
                    args.Add(ex.ProblemDetails.Detail.Trim('.'));
                }

                if (ex.ProblemDetails.Status.HasValue)
                {
                    templateBuilder.Append(" Статус-код: {StatusCode}.");
                    args.Add(ex.ProblemDetails.Status.Value.ToString());
                }

                if (ex.ProblemDetails.Extensions?.Keys?.Count > 0)
                {
                    var extensionsJson = Json.Serialize(ex.ProblemDetails.Extensions);

                    if (!string.IsNullOrEmpty(extensionsJson))
                    {
                        templateBuilder.AppendLine();
                        templateBuilder.Append(" Проблемные значения: {Extensions}");
                        args.Add(extensionsJson);
                    }
                }

                if (severity == LogLevel.Error)
                {
                    _logger.Log(severity, ex, templateBuilder.ToString(), args.ToArray());
                }
                else
                {
                    _logger.Log(severity, templateBuilder.ToString(), args.ToArray());
                }
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
            switch (exception)
            {
                case ValidationException ex:
                {
                    return new ProblemDetails
                    {
                        Type = _problemDetailTypes[StatusCodes.Status400BadRequest],
                        Status = StatusCodes.Status400BadRequest,
                        Title = ex.Message,
                        Detail = ex.ValidationResult?.ErrorMessage
                    };
                }
                case TaskCanceledException _:
                {
                    return new ProblemDetails
                    {
                        Type = _problemDetailTypes[StatusCodes.Status408RequestTimeout],
                        Status = StatusCodes.Status408RequestTimeout,
                        Title = ErrorTexts.TaskCanceled
                    };
                }
                case HttpRequestException _:
                {
                    return new ProblemDetails
                    {
                        Type = _problemDetailTypes[StatusCodes.Status502BadGateway],
                        Status = StatusCodes.Status502BadGateway,
                        Title = ErrorTexts.RelatedSerivce
                    };
                }
                default:
                {
                    return new ProblemDetails
                    {
                        Type = _problemDetailTypes[StatusCodes.Status500InternalServerError],
                        Status = StatusCodes.Status500InternalServerError,
                        Title = ErrorTexts.Internal
                    };
                }
            }
        }
    }
}