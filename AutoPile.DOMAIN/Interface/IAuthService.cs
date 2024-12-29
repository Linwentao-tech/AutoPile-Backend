using AutoPile.DOMAIN.DTOs.Requests;
using AutoPile.DOMAIN.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;

namespace AutoPile.SERVICE.Services
{
    public interface IAuthService
    {
        Task<UserResponseDTO> SignupAsync([FromBody] UserSignupDTO userSignupDTO);

        Task<UserResponseDTO> SigninAsync(UserSigninDTO userSigninDTO);

        Task<UserInfoResponseDTO> GetUserInfoAsync(string userId);

        Task<string> SendEmailConfirmationTokenAsync(string email);

        Task<bool> VerifyEmailConfirmationTokenAsync(string email, string token);

        Task UpdateUserInfoAsync(UserUpdateInfoDTO userUpdateInfoDTO, string userId);

        Task<string> SendResetPasswordTokenAsync(string email);

        Task ResetPasswordAsync(UserResetPasswordDTO userResetPasswordDTO);

        Task ValidatePasswordResetTokenAsync(string email, string token);
    }
}