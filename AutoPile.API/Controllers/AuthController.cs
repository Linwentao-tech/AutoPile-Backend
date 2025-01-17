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
    /// <summary>
    /// Controller for handling user authentication and account management
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        /// <summary>
        /// Initializes a new instance of the AuthController
        /// </summary>
        /// <param name="authService">The authentication service</param>
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Registers a new user
        /// </summary>
        /// <param name="userSignupDTO">The user registration information</param>
        /// <returns>The newly created user information</returns>
        /// <response code="200">Returns the newly created user</response>
        /// <response code="400">If the registration information is invalid</response>
        [HttpPost("SignupUser", Name = "SignupUser")]
        public async Task<IActionResult> SignupUser([FromBody] UserSignupDTO userSignupDTO)
        {
            var (userResponseDTO, token) = await _authService.SignupUserAsync(userSignupDTO);
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddHours(3)
            };
            Response.Cookies.Append("AuthToken", token, cookieOptions);
            return ApiResponse<UserResponseDTO>.OkResult(userResponseDTO);
        }

        /// <summary>
        /// Registers a new admin
        /// </summary>
        /// <param name="userSignupDTO">The admin registration information</param>
        /// <returns>The newly created admin information</returns>
        /// <response code="200">Returns the newly created user</response>
        /// <response code="400">If the registration information is invalid</response>
        [HttpPost("SignupAdmin", Name = "SignupAdmin")]
        public async Task<IActionResult> SignupAdmin([FromBody] UserSignupDTO userSignupDTO)
        {
            var (userResponseDTO, token) = await _authService.SignupAdminAsync(userSignupDTO);
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddHours(3)
            };
            Response.Cookies.Append("AuthToken", token, cookieOptions);
            return ApiResponse<UserResponseDTO>.OkResult(userResponseDTO);
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token
        /// </summary>
        /// <param name="userSigninDTO">The user login credentials</param>
        /// <returns>User information and authentication token</returns>
        /// <response code="200">Returns the user information and token</response>
        /// <response code="400">If the credentials are invalid</response>
        [HttpPost("Signin", Name = "Signin")]
        public async Task<IActionResult> Signin([FromBody] UserSigninDTO userSigninDTO)
        {
            var (userResponseDTO, token) = await _authService.SigninAsync(userSigninDTO);
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddHours(3)
            };
            Response.Cookies.Append("AuthToken", token, cookieOptions);
            return ApiResponse<UserResponseDTO>.OkResult(userResponseDTO);
        }

        /// <summary>
        /// Retrieves the current user's information
        /// </summary>
        /// <returns>Detailed user information</returns>
        /// <response code="200">Returns the user information</response>
        /// <response code="401">If the user is not authorized</response>
        /// <response code="404">If the user is not found</response>
        [HttpGet("GetUserInfoById", Name = "GetUserInfoById")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> GetUserInfoById()
        {
            var userId = HttpContext.Items["UserId"]?.ToString();
            var userInfoResponseDTO = await _authService.GetUserInfoAsync(userId);
            return ApiResponse<UserInfoResponseDTO>.OkResult(userInfoResponseDTO);
        }

        /// <summary>
        /// Sends an email confirmation link to the specified email address
        /// </summary>
        /// <param name="email">The email address to send the confirmation link to</param>
        /// <returns>A confirmation token</returns>
        /// <response code="200">Returns the confirmation token</response>
        /// <response code="401">If the user is not authorized</response>
        /// <response code="400">If the email is invalid</response>
        [HttpGet("SendEmailConfirmationLink", Name = "SendEmailConfirmationLink")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> SendEmailConfirmationLink([FromQuery] string email)
        {
            var userId = HttpContext.Items["UserId"]?.ToString();
            var token = await _authService.SendEmailConfirmationTokenAsync(email, userId);
            return ApiResponse<object>.OkResult(new { token });
        }

        /// <summary>
        /// Verifies an email confirmation token
        /// </summary>
        /// <param name="token">The confirmation token</param>
        /// <param name="email">The email address to verify</param>
        /// <returns>HTML response indicating verification status</returns>
        /// <response code="200">Returns HTML confirmation page</response>
        /// <response code="400">If the token or email is invalid</response>
        [HttpGet("VerifyEmailConfirmationLink", Name = "VerifyEmailConfirmationLink")]
        public async Task<IActionResult> VerifyEmailConfirmationLink([FromQuery] string token, [FromQuery] string email)
        {
            var isValid = await _authService.VerifyEmailConfirmationTokenAsync(token, email);
            return Content(EmailConfirmationHtmlTemplates.GetEmailConfirmationHtml(isValid), "text/html");
        }

        /// <summary>
        /// Updates the current user's information
        /// </summary>
        /// <param name="userUpdateInfoDTO">The updated user information</param>
        /// <returns>A success message</returns>
        /// <response code="200">If the update was successful</response>
        /// <response code="401">If the user is not authorized</response>
        /// <response code="400">If the update information is invalid</response>
        [HttpPut("UpdateUserInfo", Name = "UpdateUserInfo")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> UpdateUserInfo([FromBody] UserUpdateInfoDTO userUpdateInfoDTO)
        {
            var userId = HttpContext.Items["UserId"]?.ToString();
            await _authService.UpdateUserInfoAsync(userUpdateInfoDTO, userId);
            return ApiResponse.OkResult("User info successfully updated");
        }

        /// <summary>
        /// Sends a password reset token to the specified email address
        /// </summary>
        /// <param name="email">The email address to send the reset token to</param>
        /// <returns>A reset token</returns>
        /// <response code="200">Returns the reset token</response>
        /// <response code="401">If the user is not authorized</response>
        /// <response code="400">If the email is invalid</response>
        [HttpGet("SendResetPasswordToken", Name = "SendResetPasswordToken")]
        public async Task<IActionResult> SendResetPasswordToken([FromQuery] string email)
        {
            var token = await _authService.SendResetPasswordTokenAsync(email);
            return ApiResponse<object>.OkResult(new { token });
        }

        /// <summary>
        /// Resets a user's password using a reset token
        /// </summary>
        /// <param name="userResetPasswordDTO">The password reset information including the new password</param>
        /// <returns>A success message</returns>
        /// <response code="200">If the password was successfully reset</response>
        /// <response code="400">If the reset information is invalid</response>
        [HttpPost("ResetPassword", Name = "ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] UserResetPasswordDTO userResetPasswordDTO)
        {
            await _authService.ResetPasswordAsync(userResetPasswordDTO);
            return ApiResponse.OkResult("Password successfully reset");
        }

        /// <summary>
        /// Validates a password reset token
        /// </summary>
        /// <param name="validateTokenRequest">The token validation request containing email and token</param>
        /// <returns>A success message if the token is valid</returns>
        /// <response code="200">If the token is valid</response>
        /// <response code="400">If the token is invalid or expired</response>
        [HttpPost("ValidatePasswordResetToken", Name = "ValidatePasswordResetTokenAsync")]
        public async Task<IActionResult> ValidatePasswordResetToken([FromBody] ValidateTokenRequest validateTokenRequest)
        {
            await _authService.ValidatePasswordResetTokenAsync(validateTokenRequest.Email, validateTokenRequest.Token);
            return ApiResponse.OkResult("Token successfully validate");
        }

        /// <summary>
        /// Signs out the current user by removing their authentication token
        /// </summary>
        /// <returns>A success message indicating the user has been logged out</returns>
        /// <response code="200">If the user was successfully logged out</response>
        /// <response code="401">If the user is not authenticated</response>
        [HttpPost("Signout", Name = "Signout")]
        [Authorize]
        public async Task<IActionResult> Signout()
        {
            Response.Cookies.Delete("AuthToken", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });
            return ApiResponse.OkResult("Logged out successfully");
        }
    }
}