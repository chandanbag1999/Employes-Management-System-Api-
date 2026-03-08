using EmployesManagementSystemApi.Models;

namespace EmployesManagementSystemApi.Repositories.Interfaces
{
    public interface IAuthRepository
    {
        Task<AppUser?> GetByEmailAsync(string email);
        Task<AppUser> CreateAsync(AppUser user);
        Task<bool> EmailExistsAsync(string email);
    }
}
