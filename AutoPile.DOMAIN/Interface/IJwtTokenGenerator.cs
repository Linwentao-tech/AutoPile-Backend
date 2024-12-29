using AutoPile.DOMAIN.Models.Entities;

namespace AutoPile.SERVICE.Utilities
{
    public interface IJwtTokenGenerator
    {
        string GenerateJwtToken(ApplicationUser user);
    }
}