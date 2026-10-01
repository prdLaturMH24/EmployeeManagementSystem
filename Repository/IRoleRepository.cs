using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.Repository
{
    /// <summary>
    /// Repository interface for accessing role and user-role data.
    /// </summary>
    public interface IRoleRepository
    {
        /// <summary>
        /// Get all roles for a specific user.
        /// </summary>
        Task<List<Role>> GetUserRolesAsync(int userId);

        /// <summary>
        /// Check if a user has a specific role.
        /// </summary>
        Task<bool> UserHasRoleAsync(int userId, string roleName);

        /// <summary>
        /// Get a role by name.
        /// </summary>
        Task<Role?> GetRoleByNameAsync(string roleName);

        /// <summary>
        /// Get a role by ID.
        /// </summary>
        Task<Role?> GetRoleByIdAsync(int roleId);

        /// <summary>
        /// Assign a role to a user.
        /// </summary>
        Task<bool> AssignRoleToUserAsync(int userId, int roleId);

        /// <summary>
        /// Remove a role from a user.
        /// </summary>
        Task<bool> RemoveRoleFromUserAsync(int userId, int roleId);

        /// <summary>
        /// Get all roles.
        /// </summary>
        Task<List<Role>> GetAllRolesAsync();

        /// <summary>
        /// Check if a user has any of the specified roles.
        /// </summary>
        Task<bool> UserHasAnyRoleAsync(int userId, params string[] roleNames);

        /// <summary>
        /// Check if a user has all of the specified roles.
        /// </summary>
        Task<bool> UserHasAllRolesAsync(int userId, params string[] roleNames);
    }
}
