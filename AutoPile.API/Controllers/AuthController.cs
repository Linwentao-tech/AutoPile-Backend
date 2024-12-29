using AutoPile.DOMAIN.DTOs.Requests;
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
            return Ok(userResponseDTO);
        }

        [HttpPost("Signin", Name = "Signin")]
        public async Task<IActionResult> Signin([FromBody] UserSigninDTO userSigninDTO)
        {
            var userResponseDTO = await _authService.SigninAsync(userSigninDTO);
            return Ok(userResponseDTO);
        }

        [HttpGet("GetUserInfoById", Name = "GetUserInfoById")]
        public async Task<IActionResult> GetUserInfoById(string userId)
        {
            var userInfoResponseDTO = await _authService.GetUserInfoAsync(userId);
            return Ok(userInfoResponseDTO);
        }

        [HttpGet("SendEmailConfirmationLink", Name = "SendEmailConfirmationLink")]
        public async Task<IActionResult> SendEmailConfirmationLink([FromQuery] string email)
        {
            var token = await _authService.SendEmailConfirmationTokenAsync(email);
            return Ok(token);
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
            return Ok(new { message = "User info successfully updated" });
        }

        [HttpGet("SendResetPasswordToken", Name = "SendResetPasswordToken")]
        public async Task<IActionResult> SendResetPasswordToken([FromQuery] string email)
        {
            var token = await _authService.SendResetPasswordTokenAsync(email);
            return Ok(token);
        }

        [HttpPost("ResetPassword", Name = "ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] UserResetPasswordDTO userResetPasswordDTO)
        {
            await _authService.ResetPasswordAsync(userResetPasswordDTO);
            return Ok(new { message = "Password successfully reset" });
        }

        [HttpPost("ValidatePasswordResetToken", Name = "ValidatePasswordResetTokenAsync")]
        public async Task<IActionResult> ValidatePasswordResetToken([FromBody] ValidateTokenRequest validateTokenRequest)
        {
            await _authService.ValidatePasswordResetTokenAsync(validateTokenRequest.Email, validateTokenRequest.Token);
            return Ok(new { message = "Token valid" });
        }
    }
}