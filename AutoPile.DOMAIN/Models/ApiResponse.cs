using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPile.DOMAIN.Models
{
    public static class ApiResponse
    {
        public static IActionResult OkResult(string message = "OK")
        {
            var response = new ApiResponse<object>
            {
                Code = 200,
                Message = message,
                Success = true,
            };
            return new OkObjectResult(response);
        }

        public static IActionResult FailResult(int code = 500, string message = "Failure")
        {
            var response = new ApiResponse<object>
            {
                Code = code,
                Message = message,
                Success = false,
            };
            return new ObjectResult(response);
        }
    }

    public class ApiResponse<T>
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public T? Data { get; set; }
        public bool Success { get; set; }

        public static IActionResult OkResult(T data, string message = "OK")
        {
            var response = new ApiResponse<T>
            {
                Code = 200,
                Message = message,
                Success = true,
                Data = data,
            };
            return new OkObjectResult(response);
        }

        public static IActionResult OkResult(string message = "OK")
        {
            var response = new ApiResponse<T>
            {
                Code = 200,
                Message = message,
                Success = true,
            };
            return new OkObjectResult(response);
        }

        public static IActionResult FailResult(int code = 500, string message = "Failure")
        {
            var response = new ApiResponse<T>
            {
                Code = code,
                Message = message,
                Success = false
            };
            return new ObjectResult(response) { StatusCode = code };
        }
    }
}