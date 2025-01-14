using AutoPile.DOMAIN.DTOs.Responses;

namespace AutoPile.DATA.Cache
{
    public interface IUserInfoCache
    {
        Task SetUserAsync(string applicationUserId, UserInfoResponseDTO userInfoResponseDTO);

        Task<UserInfoResponseDTO?> GetUserAsync(string applicationUserId);
    }
}