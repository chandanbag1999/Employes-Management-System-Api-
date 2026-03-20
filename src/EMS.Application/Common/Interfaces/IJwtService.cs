using EMS.Domain.Entities.Identity;

namespace EMS.Application.Common.Interfaces;

public interface IJwtService
{
    string GenerateToken(AppUser user);
    DateTime GetExpiryTime();
}