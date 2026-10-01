using Microsoft.AspNetCore.Authorization;

namespace EmployeeManagementSystem.Authorization.Requirements
{
    /// <summary>
    /// Authorization requirement for hierarchical role checking.
    /// Supports role hierarchy: Admin > Manager > Associate
    /// </summary>
    public class HierarchicalRoleRequirement : IAuthorizationRequirement
    {
        public string MinimumRole { get; }

        /// <summary>
        /// Creates a new hierarchical role requirement.
        /// </summary>
        /// <param name="minimumRole">The minimum role required (Admin, Manager, or Associate)</param>
        public HierarchicalRoleRequirement(string minimumRole)
        {
            MinimumRole = minimumRole ?? throw new ArgumentNullException(nameof(minimumRole));
        }
    }
}
