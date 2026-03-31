using EMS.Domain.Entities.Identity;

namespace EMS.Application.Common.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(AppUser user);
    string GenerateRefreshToken();
    DateTime GetAccessTokenExpiry();
    DateTime GetRefreshTokenExpiry();
    int? GetUserIdFromToken(string token);
}