using AutoPile.DOMAIN.DTOs.Requests;
using AutoPile.DOMAIN.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;

namespace AutoPile.SERVICE.Services
{
    public interface IAuthService
    {
        Task<(UserResponseDTO, string)> SignupAdminAsync(UserSignupDTO userSignupDTO);

        Task<(UserResponseDTO, string)> SignupUserAsync([FromBody] UserSignupDTO userSignupDTO);

        Task<(UserResponseDTO, string)> SigninAsync(UserSigninDTO userSigninDTO);

        Task<UserInfoResponseDTO> GetUserInfoAsync(string userId);

        Task<string> SendEmailConfirmationTokenAsync(string email, string userId);

        Task<bool> VerifyEmailConfirmationTokenAsync(string email, string token);

        Task UpdateUserInfoAsync(UserUpdateInfoDTO userUpdateInfoDTO, string userId);

        Task<string> SendResetPasswordTokenAsync(string email);

        Task ResetPasswordAsync(UserResetPasswordDTO userResetPasswordDTO);

        Task ValidatePasswordResetTokenAsync(string email, string token);
    }
}