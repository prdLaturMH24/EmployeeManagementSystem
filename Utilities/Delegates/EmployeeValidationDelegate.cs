using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.Utilities.Delegates
{
    /// <summary>
    /// Demonstrates C# Delegate Types: Custom Delegates, Func<>, Predicate<>
    /// 
    /// USE CASE: Employee Validation Pipeline
    /// =====================================
    /// Delegates allow you to pass validation logic as parameters, enabling:
    /// 1. Custom validation rules without modifying code
    /// 2. Composable validation pipelines
    /// 3. Flexible filtering criteria for employee queries
    /// 4. Separation of validation logic from business logic
    /// 
    /// KEY CONCEPTS:
    /// - Custom Delegate: Define specific method signatures for your domain
    /// - Func<T, TResult>: Generic delegate for transformations/validations (returns value)
    /// - Predicate<T>: Generic delegate for filtering (returns bool) - essentially Func<T, bool>
    /// - Action<T>: Generic delegate for operations without return value
    /// </summary>
    public class EmployeeValidationDelegate
    {
        /// <summary>
        /// CUSTOM DELEGATE TYPE
        /// Defines a custom delegate for employee validation.
        /// This is useful when you want a domain-specific type with clear intent.
        /// 
        /// Signature: Takes an Employee, returns a tuple (isValid, errorMessage)
        /// Use Case: Complex validations where you need both result and reason
        /// </summary>
        public delegate (bool isValid, string errorMessage) EmployeeValidationRule(Employee employee);

        /// <summary>
        /// CUSTOM DELEGATE TYPE for notification
        /// Demonstrates another custom delegate pattern for domain-specific scenarios.
        /// Use Case: Validation feedback without throwing exceptions
        /// </summary>
        public delegate void ValidationResultCallback(Employee employee, bool isValid, string message);

        // ========== VALIDATION RULES USING CUSTOM DELEGATES ==========

        /// <summary>
        /// Validates that employee has non-empty last name.
        /// This is a named method that matches the EmployeeValidationRule delegate signature.
        /// </summary>
        public static (bool isValid, string errorMessage) ValidateEmployeeFirstName(Employee employee)
        {
            if (string.IsNullOrWhiteSpace(employee.FirstName))
                return (false, "First name cannot be empty");

            return (true, "First Name validation passed");
        }

        /// <summary>
        /// Validates that employee has non-empty last name.
        /// This is a named method that matches the EmployeeValidationRule delegate signature.
        /// </summary>
        public static (bool isValid, string errorMessage) ValidateEmployeeLastName(Employee employee)
        {
            if (string.IsNullOrWhiteSpace(employee.LastName))
                return (false, "Last name cannot be empty");

            return (true, "Last Name validation passed");
        }

        /// <summary>
        /// Validates employee ID format (custom rule: must be non-empty).
        /// Demonstrates reusability of validation methods.
        /// </summary>
        public static (bool isValid, string errorMessage) ValidateEmployeeId(Employee employee)
        {
            if (string.IsNullOrWhiteSpace(employee.AssociateId))
                return (false, "Associate ID cannot be empty");

            return (true, "ID validation passed");
        }

        /// <summary>
        /// Validates that employee has a valid phone number in details.
        /// </summary>
        /// <param name="employee"></param>
        /// <returns></returns>
        public static (bool isValid, string errorMessage) ValidateEmployeePhoneNumber(Employee employee)
        {
            if (string.IsNullOrWhiteSpace(employee.EmployeeDetail?.PhoneNumber))
                return (false, "Phone number cannot be empty");

            return (true, "Phone number validation passed");
        }

        // ========== VALIDATION USING FUNC<> GENERIC DELEGATE ==========

        /// <summary>
        /// Collection of reusable validation predicates using Func<Employee, bool>
        /// 
        /// Func<T, TResult> is used when:
        /// - You need a transformation or condition check with a return value
        /// - You want to use standard .NET types (easier than custom delegates)
        /// - The logic is simple and doesn't need domain-specific naming
        /// 
        /// Benefits: Can be easily combined with LINQ, passed to standard methods
        /// </summary>
        public class ValidationPredicates
        {
            /// <summary>
            /// Predicate: Employee has a valid Associate ID.
            /// Using Func<Employee, bool> for simple boolean checks.
            /// </summary>
            public static readonly Func<Employee, bool> HasValidAssociateId = emp =>
                !string.IsNullOrWhiteSpace(emp.AssociateId) && emp.AssociateId.Length >= 3;

            /// <summary>
            /// Predicate: Employee has complete name.
            /// Demonstrates lambda expression with multiple conditions.
            /// </summary>
            public static readonly Func<Employee, bool> HasCompleteName = emp =>
                !string.IsNullOrWhiteSpace(emp.FirstName) &&
                !string.IsNullOrWhiteSpace(emp.LastName);

            /// <summary>
            /// Predicate: Employee has associated details.
            /// Used for queries requiring detailed employee information.
            /// </summary>
            public static readonly Func<Employee, bool> HasEmployeeDetails = emp =>
                emp.EmployeeDetail != null;

            /// <summary>
            /// Predicate: Employee has valid email in details.
            /// Demonstrates nullable reference handling.
            /// </summary>
            public static readonly Func<Employee, bool> HasValidEmail = emp =>
                emp.EmployeeDetail != null &&
                !string.IsNullOrWhiteSpace(emp.EmployeeDetail.Email) &&
                emp.EmployeeDetail.Email.Contains("@");

            /// <summary>
            /// Composite predicate: Employee passes complete validation.
            /// Demonstrates combining multiple predicates.
            /// </summary>
            public static readonly Func<Employee, bool> IsCompleteEmployee = emp =>
                HasValidAssociateId(emp) &&
                HasCompleteName(emp) &&
                HasEmployeeDetails(emp) &&
                HasValidEmail(emp);
        }

        // ========== VALIDATION PIPELINE ==========

        /// <summary>
        /// Demonstrates a flexible validation pipeline using delegates.
        /// 
        /// KEY PATTERN: Composition of validation delegates
        /// - Allows adding/removing validations at runtime
        /// - No need to modify validation logic itself
        /// - Each rule is independent and reusable
        /// </summary>
        public class EmployeeValidationPipeline
        {
            private readonly List<EmployeeValidationRule> _validationRules = [];
            private readonly List<ValidationResultCallback> _callbacks = [];

            /// <summary>
            /// Adds a validation rule to the pipeline.
            /// Use Case: Register custom validation logic without modifying core code.
            /// </summary>
            public void AddValidationRule(EmployeeValidationRule rule)
            {
                _validationRules.Add(rule);
            }

            /// <summary>
            /// Registers a callback for validation results.
            /// Use Case: Receive validation feedback (e.g., logging, UI updates).
            /// </summary>
            public void RegisterValidationCallback(ValidationResultCallback callback)
            {
                _callbacks.Add(callback);
            }

            /// <summary>
            /// Executes all validation rules and invokes callbacks.
            /// Returns: Combined result of all validations.
            /// 
            /// Demonstrates:
            /// - Invoking delegates (validation rules)
            /// - Invoking multicast delegates (multiple callbacks)
            /// - Short-circuit validation (stops on first failure)
            /// </summary>
            public (bool allValid, List<string> errors) ValidateEmployee(Employee employee)
            {
                var errors = new List<string>();

                // Execute each validation rule
                foreach (var rule in _validationRules)
                {
                    var (isValid, message) = rule(employee);

                    if (!isValid)
                        errors.Add(message);

                    // Invoke all callbacks
                    foreach (var callback in _callbacks)
                    {
                        callback(employee, isValid, message);
                    }
                }

                return (errors.Count == 0, errors);
            }

            /// <summary>
            /// Creates a standard validation pipeline.
            /// Demonstrates preconfigured pipelines with common rules.
            /// </summary>
            public static EmployeeValidationPipeline CreateStandardPipeline()
            {
                var pipeline = new EmployeeValidationPipeline();
                pipeline.AddValidationRule(ValidateEmployeeId);
                pipeline.AddValidationRule(ValidateEmployeeFirstName);
                pipeline.AddValidationRule(ValidateEmployeeLastName);
                pipeline.AddValidationRule(ValidateEmployeePhoneNumber);
                return pipeline;
            }
        }

        // ========== PRACTICAL USAGE EXAMPLES ==========

        /// <summary>
        /// Example: Using Predicate<Employee> with LINQ for filtering.
        /// 
        /// USE CASE: Filter employees based on dynamic criteria
        /// - No need for multiple hardcoded filter methods
        /// - Criteria can be changed at runtime
        /// - Composable filtering logic
        /// </summary>
        public static List<Employee> FilterEmployees(
            List<Employee> employees,
            Func<Employee, bool> predicate)
        {
            return employees.Where(predicate).ToList();
        }

        /// <summary>
        /// Example: Building complex validation without custom validators.
        /// 
        /// USE CASE: Domain-specific validation combining multiple rules
        /// Demonstrates: Composing validation logic using delegates
        /// </summary>
        public static bool IsEmployeeValidForPayroll(Employee employee)
        {
            // Combines multiple validation predicates using AND logic
            return ValidationPredicates.HasCompleteName(employee) &&
                   ValidationPredicates.HasEmployeeDetails(employee) &&
                   employee.EmployeeDetail!.Salary > 0;
        }

        /// <summary>
        /// Example: Custom validator function using Func<>.
        /// 
        /// USE CASE: Apply custom logic dynamically
        /// Demonstrates: Receiving validation logic as parameter
        /// </summary>
        public static (bool isValid, string result) ValidateWith(
            Employee employee,
            Func<Employee, (bool, string)> customValidator)
        {
            return customValidator(employee);
        }
    }
}
