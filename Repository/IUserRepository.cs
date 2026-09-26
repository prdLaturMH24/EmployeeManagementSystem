using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.Repository
{
    public interface IUserRepository
    {
        Task<List<AppUser>> ListUsersAsync();
        Task<AppUser?> GetUserByIdAsync(int id);
        Task<AppUser?> GetUserByUsernameOrEmailAsync(string username);
        Task<bool> UserExistsAsync(string userName, string email);
        Task<AppUser?> AddUserAsync(AppUser appUser);
    }
}
