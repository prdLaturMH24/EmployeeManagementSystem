using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementSystem.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly EmployeeDbContext _context;
        public UserRepository(EmployeeDbContext context)
        {
            _context = context;
        }

        public async Task<AppUser?> GetUserByIdAsync(int id)
        {
            return await _context.AppUsers
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<AppUser?> GetUserByUsernameOrEmailAsync(string username)
        {
            return await _context.AppUsers
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.UserName == username || u.Email == username);
        }

        public async Task<bool> UserExistsAsync(string userName, string email)
        {
            return await _context.AppUsers.AnyAsync(u => u.UserName == userName || u.Email == email);
        }

        public async Task<List<AppUser>> ListUsersAsync()
        {
            return await _context.AppUsers
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .ToListAsync();
        }

        public async Task<AppUser?> AddUserAsync(AppUser appUser)
        {
            _context.AppUsers.Add(appUser);
            var result = await _context.SaveChangesAsync();
            return result > 0 ? appUser : null;
        }
    }
}
