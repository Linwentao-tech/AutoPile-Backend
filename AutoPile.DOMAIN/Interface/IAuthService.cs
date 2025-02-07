using AutoPile.DOMAIN.DTOs.Requests;
using AutoPile.DOMAIN.DTOs.Responses;
using AutoPile.DOMAIN.Models.Entities;
using Microsoft.AspNetCore.Mvc;

namespace AutoPile.SERVICE.Services
{
    public interface IAuthService
    {
        Task<(UserResponseDTO, string, string)> SignupAdminAsync(UserSignupDTO userSignupDTO);

        Task<(UserResponseDTO, string, string)> SignupUserAsync([FromBody] UserSignupDTO userSignupDTO);

        Task<(UserResponseDTO, string, string)> SigninAsync(UserSigninDTO userSigninDTO);

        Task<UserInfoResponseDTO> GetUserInfoAsync(string userId);

        Task<string> SendEmailConfirmationTokenAsync(string email, string userId);

        Task<bool> VerifyEmailConfirmationTokenAsync(string email, string token);

        Task UpdateUserInfoAsync(UserUpdateInfoDTO userUpdateInfoDTO, string userId);

        Task<string> SendResetPasswordTokenAsync(string email);

        Task ResetPasswordAsync(UserResetPasswordDTO userResetPasswordDTO);

        Task ValidatePasswordResetTokenAsync(string email, string token);

        Task RevokeRefreshTokenAsync(string userId);

        Task<TokenRefreshResponse> RefreshTokenAsync(string refreshToken);

        Task UpdateUserRefreshTokenAsync(ApplicationUser user, string refreshToken);
    }
}