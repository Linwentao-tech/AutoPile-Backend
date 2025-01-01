using AutoPile.DOMAIN.DTOs.Requests;
using AutoPile.DOMAIN.DTOs.Responses;
using AutoPile.DOMAIN.Models;
using AutoPile.DOMAIN.Models.Entities;
using AutoPile.SERVICE.Services;
using AutoPile.SERVICE.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AutoPile.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("Signup", Name = "Signup")]
        public async Task<IActionResult> Signup([FromBody] UserSignupDTO userSignupDTO)
        {
            var userResponseDTO = await _authService.SignupAsync(userSignupDTO);
            return ApiResponse<UserResponseDTO>.OkResult(userResponseDTO);
        }

        [HttpPost("Signin", Name = "Signin")]
        public async Task<IActionResult> Signin([FromBody] UserSigninDTO userSigninDTO)
        {
            var userResponseDTO = await _authService.SigninAsync(userSigninDTO);
            return ApiResponse<UserResponseDTO>.OkResult(userResponseDTO);
        }

        [HttpGet("GetUserInfoById", Name = "GetUserInfoById")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetUserInfoById()
        {
            var userId = HttpContext.Items["UserId"]?.ToString();
            var userInfoResponseDTO = await _authService.GetUserInfoAsync(userId);
            return ApiResponse<UserInfoResponseDTO>.OkResult(userInfoResponseDTO);
        }

        [HttpGet("SendEmailConfirmationLink", Name = "SendEmailConfirmationLink")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> SendEmailConfirmationLink([FromQuery] string email)
        {
            var userId = HttpContext.Items["UserId"]?.ToString();
            var token = await _authService.SendEmailConfirmationTokenAsync(email, userId);
            return ApiResponse<object>.OkResult(new { token });
        }

        [HttpGet("VerifyEmailConfirmationLink", Name = "VerifyEmailConfirmationLink")]
        public async Task<IActionResult> VerifyEmailConfirmationLink([FromQuery] string token, [FromQuery] string email)
        {
            var isValid = await _authService.VerifyEmailConfirmationTokenAsync(token, email);
            return Content(EmailConfirmationHtmlTemplates.GetEmailConfirmationHtml(isValid), "text/html");
        }

        [HttpPut("UpdateUserInfo", Name = "UpdateUserInfo")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> UpdateUserInfo([FromBody] UserUpdateInfoDTO userUpdateInfoDTO)
        {
            var userId = HttpContext.Items["UserId"]?.ToString();
            await _authService.UpdateUserInfoAsync(userUpdateInfoDTO, userId);
            return ApiResponse.OkResult("User info successfully updated");
        }

        [HttpGet("SendResetPasswordToken", Name = "SendResetPasswordToken")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> SendResetPasswordToken([FromQuery] string email)
        {
            var userId = HttpContext.Items["UserId"]?.ToString();
            var token = await _authService.SendResetPasswordTokenAsync(email, userId);
            return ApiResponse<object>.OkResult(new { token });
        }

        [HttpPost("ResetPassword", Name = "ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] UserResetPasswordDTO userResetPasswordDTO)
        {
            await _authService.ResetPasswordAsync(userResetPasswordDTO);
            return ApiResponse.OkResult("Password successfully reset");
        }

        [HttpPost("ValidatePasswordResetToken", Name = "ValidatePasswordResetTokenAsync")]
        public async Task<IActionResult> ValidatePasswordResetToken([FromBody] ValidateTokenRequest validateTokenRequest)
        {
            await _authService.ValidatePasswordResetTokenAsync(validateTokenRequest.Email, validateTokenRequest.Token);
            return ApiResponse.OkResult("Token successfully validate");
        }
    }
}