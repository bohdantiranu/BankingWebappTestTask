using BankingWebApp.Api.Dto.Responses;
using BankingWebApp.Api.Exceptions;
using System.Net;
using System.Text.Json;

namespace BankingWebApp.Api.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            var response = new BaseResponse<object>();

            switch (exception)
            {
                case FluentValidation.ValidationException validationException:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Error = new ErrorResponse
                    {
                        StatusCode = context.Response.StatusCode,
                        Message = "Validation failed.",
                        Errors = validationException.Errors
                    };

                    _logger.LogError("Validation failed: {Errors}", validationException.Errors);
                    break;
                case AccountNotFoundException accountNotFoundException:
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.Error = new ErrorResponse
                    {
                        StatusCode = context.Response.StatusCode,
                        Message = accountNotFoundException.Message
                    };

                    _logger.LogError("Account not found: {Message}", accountNotFoundException.Message);
                    break;
                case InsufficientFundsException insufficientFundsException:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Error = new ErrorResponse
                    {
                        StatusCode = context.Response.StatusCode,
                        Message = insufficientFundsException.Message
                    };

                    _logger.LogError("Insufficient funds: {Message}", insufficientFundsException.Message);
                    break;
                case InvalidOperationException invalidOperationException:
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    response.Error = new ErrorResponse
                    {
                        StatusCode = context.Response.StatusCode,
                        Message = invalidOperationException.Message
                    };

                    _logger.LogError("An exception occurred: {Ex}", invalidOperationException.Message);
                    break;
                case Exception ex:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    response.Error = new ErrorResponse 
                    { 
                        StatusCode = (int)HttpStatusCode.InternalServerError, 
                        Message = "Internal Server Error." 
                    };
                    
                    _logger.LogError("An exception occurred: {Ex}", ex);         
                    break;
            }
            
            response.IsSuccess = false;

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
