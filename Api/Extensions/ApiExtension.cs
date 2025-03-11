using BankingWebApp.Api.Dto.Responses;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace BankingWebApp.Api.Extensions
{
    public static class ApiExtension
    {
        public static ActionResult<BaseResponse<T>> OkResult<T>(this ControllerBase controller, T data, 
            string message = null)
        {
            return controller.Ok(new BaseResponse<T> 
            { 
                IsSuccess = true, 
                Data = data, 
                Message = message 
            });
        }

        public static ActionResult<BaseResponse<T>> CreatedResult<T>(this ControllerBase controller, T data, 
            string message = null)
        {
            return controller.CreatedAtAction(null, new BaseResponse<T> 
            { 
                IsSuccess = true, 
                Data = data, 
                Message = message 
            });
        }

        public static ActionResult<BaseResponse<T>> NotFoundResult<T>(this ControllerBase controller, 
            string errorMessage)
        {
            return controller.NotFound(new BaseResponse<T>
            {
                IsSuccess = false,
                Error = new ErrorResponse { StatusCode = (int)HttpStatusCode.NotFound, Message = errorMessage }
            });
        }
    }
}
