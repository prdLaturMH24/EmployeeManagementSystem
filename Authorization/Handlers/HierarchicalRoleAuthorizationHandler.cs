using EmployeeManagementSystem.Authorization.Requirements;
using EmployeeManagementSystem.Common.Constants;
using EmployeeManagementSystem.Repository;
using Microsoft.AspNetCore.Authorization;

namespace EmployeeManagementSystem.Authorization.Handlers
{
    /// <summary>
    /// Authorization handler for hierarchical role-based authorization.
    /// Implements role hierarchy: Admin (3) > Manager (2) > Associate (1)
    /// </summary>
    public class HierarchicalRoleAuthorizationHandler : AuthorizationHandler<HierarchicalRoleRequirement>
    {
        private readonly IRoleRepository _roleRepository;

        // Role hierarchy levels
        private static readonly Dictionary<string, int> RoleHierarchy = new()
        {
            { RoleConstants.Admin, 3 },
            { RoleConstants.Manager, 2 },
            { RoleConstants.Associate, 1 }
        };

        public HierarchicalRoleAuthorizationHandler(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
        }

        /// <summary>
        /// Handles the authorization requirement by checking if user has sufficient role level.
        /// </summary>
        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            HierarchicalRoleRequirement requirement)
        {
            if (context.User != null && context.User.Identity?.IsAuthenticated == true)
            {
                // Extract user ID from claims
                var userIdClaim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (int.TryParse(userIdClaim, out int userId))
                {
                    // Get user's roles from database
                    var userRoles = await _roleRepository.GetUserRolesAsync(userId);

                    if (!RoleHierarchy.TryGetValue(requirement.MinimumRole, out var requiredLevel))
                    {
                        // Invalid role specified in requirement
                        return;
                    }

                    // Check if user has any role that meets or exceeds the minimum requirement
                    foreach (var userRole in userRoles)
                    {
                        if (RoleHierarchy.TryGetValue(userRole.Name, out var userLevel) && userLevel >= requiredLevel)
                        {
                            context.Succeed(requirement);
                            return;
                        }
                    }
                }
            }

            // Authorization failed - context.Fail() is not needed; just don't call context.Succeed()
        }
    }
}
