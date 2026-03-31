using EMS.Domain.Entities.Identity;

namespace EMS.Application.Modules.Identity.Interfaces;

public interface IAuthRepository
{
    Task<AppUser?> GetByEmailAsync(string email);
    Task<AppUser?> GetByIdAsync(int id);
    Task<AppUser?> GetByIdWithTokensAsync(int id);
    Task<IEnumerable<AppUser>> GetAllAsync();
    Task<AppUser> CreateAsync(AppUser user);
    Task<bool> EmailExistsAsync(string email);
    Task SaveChangesAsync();
}