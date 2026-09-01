using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Credilike.Core.DTOs;
using Credilike.Core.Exceptions;

namespace Credilike.Api.Middlewares
{
    /// <summary>
    /// Middleware global para garantizar que todos los errores imprevistos o excepciones
    /// de dominio retornen un JSON con estructura uniforme (codigo, mensaje, detalle).
    /// </summary>
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
            context.Response.ContentType = "application/json";

            ErrorResponseDto errorDto;
            HttpStatusCode statusCode;

            switch (exception)
            {
                case NotFoundException nfe:
                    statusCode = HttpStatusCode.NotFound;
                    errorDto = new ErrorResponseDto(nfe.Codigo, nfe.Message, nfe.Detalle);
                    break;

                case TenantForbiddenException tfe:
                    statusCode = HttpStatusCode.Forbidden;
                    errorDto = new ErrorResponseDto(tfe.Codigo, tfe.Message, tfe.Detalle);
                    break;

                case InvalidOperationDomainException iode:
                    statusCode = HttpStatusCode.BadRequest;
                    errorDto = new ErrorResponseDto(iode.Codigo, iode.Message, iode.Detalle);
                    break;

                case DomainException de:
                    statusCode = HttpStatusCode.BadRequest;
                    errorDto = new ErrorResponseDto(de.Codigo, de.Message, de.Detalle);
                    break;

                case ArgumentException ae:
                    statusCode = HttpStatusCode.BadRequest;
                    errorDto = new ErrorResponseDto("ARGUMENT_ERROR", ae.Message, ae.ParamName ?? "Parámetro inválido.");
                    break;

                default:
                    statusCode = HttpStatusCode.InternalServerError;
                    errorDto = new ErrorResponseDto(
                        "INTERNAL_SERVER_ERROR",
                        "Ha ocurrido un error no controlado en el servidor.",
                        exception.Message
                    );
                    break;
            }

            context.Response.StatusCode = (int)statusCode;

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(errorDto, jsonOptions);
            return context.Response.WriteAsync(json);
        }
    }
}
