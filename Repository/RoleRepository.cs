using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementSystem.Repository
{
    /// <summary>
    /// Repository implementation for accessing role and user-role data.
    /// </summary>
    public class RoleRepository : IRoleRepository
    {
        private readonly EmployeeDbContext _context;

        public RoleRepository(EmployeeDbContext context)
        {
            _context = context;
        }

        public async Task<List<Role>> GetUserRolesAsync(int userId)
        {
            return await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Include(ur => ur.Role)
                .Select(ur => ur.Role)
                .ToListAsync();
        }

        public async Task<bool> UserHasRoleAsync(int userId, string roleName)
        {
            return await _context.UserRoles
                .AnyAsync(ur => ur.UserId == userId && ur.Role.Name == roleName);
        }

        public async Task<Role?> GetRoleByNameAsync(string roleName)
        {
            return await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == roleName);
        }

        public async Task<Role?> GetRoleByIdAsync(int roleId)
        {
            return await _context.Roles.FindAsync(roleId);
        }

        public async Task<bool> AssignRoleToUserAsync(int userId, int roleId)
        {
            // Check if user already has this role
            var userRoleExists = await _context.UserRoles
                .AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

            if (userRoleExists)
            {
                return false; // Role already assigned
            }

            var userRole = new UserRole
            {
                UserId = userId,
                RoleId = roleId
            };

            _context.UserRoles.Add(userRole);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> RemoveRoleFromUserAsync(int userId, int roleId)
        {
            var userRole = await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

            if (userRole == null)
            {
                return false; // Role assignment not found
            }

            _context.UserRoles.Remove(userRole);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<Role>> GetAllRolesAsync()
        {
            return await _context.Roles.ToListAsync();
        }

        public async Task<bool> UserHasAnyRoleAsync(int userId, params string[] roleNames)
        {
            return await _context.UserRoles
                .AnyAsync(ur => ur.UserId == userId && roleNames.Contains(ur.Role.Name));
        }

        public async Task<bool> UserHasAllRolesAsync(int userId, params string[] roleNames)
        {
            var userRoleCount = await _context.UserRoles
                .Where(ur => ur.UserId == userId && roleNames.Contains(ur.Role.Name))
                .Select(ur => ur.Role.Name)
                .Distinct()
                .CountAsync();

            return userRoleCount == roleNames.Length;
        }
    }
}
