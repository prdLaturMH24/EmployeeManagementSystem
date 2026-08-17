using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.Utilities.Delegates
{
    /// <summary>
    /// Demonstrates C# Delegate Callback Pattern and Async-Friendly Delegates
    /// 
    /// USE CASE: Employee Salary Processing with Audit Logging
    /// ========================================================
    /// Callbacks are used when you need to:
    /// 1. Execute logic after an operation completes
    /// 2. Return results through a delegate parameter
    /// 3. Decouple business logic from cross-cutting concerns (logging, audit)
    /// 4. Support async operations without blocking
    /// 5. Pass multiple results back to caller
    /// 
    /// KEY CONCEPTS:
    /// - Callback Delegate: Method passed as parameter, called later
    /// - Async/Await Compatibility: Delegates work with async methods
    /// - Composition: Chaining multiple callbacks for complex workflows
    /// - Error Handling: Callbacks enable custom error processing
    /// </summary>
    public class EmployeeProcessingDelegate
    {
        /// <summary>
        /// CUSTOM CALLBACK DELEGATE
        /// Called after a salary modification is processed.
        /// Returns processing result with context.
        /// 
        /// Use Case: Log, audit, notify about salary changes
        /// Parameters: employee, previous salary, new salary, timestamp
        /// </summary>
        public delegate void SalaryModificationCallback(
            Employee employee,
            decimal previousSalary,
            decimal newSalary,
            DateTime modificationTime);

        /// <summary>
        /// CALLBACK DELEGATE for processing errors.
        /// Enables custom error handling at point of use.
        /// 
        /// Use Case: Different error handling strategies per caller
        /// </summary>
        public delegate void ProcessingErrorCallback(
            string employeeId,
            string operationName,
            Exception exception);

        /// <summary>
        /// CALLBACK DELEGATE for processing progress.
        /// Used for long-running operations with progress updates.
        /// 
        /// Use Case: Batch processing, reporting completion percentage
        /// </summary>
        public delegate void ProcessingProgressCallback(
            int processedCount,
            int totalCount,
            string currentEmployeeId);

        /// <summary>
        /// Complex callback delegate carrying multiple results.
        /// Demonstrates passing rich context through callbacks.
        /// </summary>
        public delegate void BatchProcessingCallback(
            BatchProcessingResult result);

        /// <summary>
        /// Represents the result of batch processing operation.
        /// </summary>
        public record BatchProcessingResult(
            int TotalProcessed,
            int SuccessfulCount,
            int FailedCount,
            List<string> FailedEmployeeIds,
            TimeSpan ProcessingDuration);

        // ========== SALARY MODIFICATION LOGGING ==========

        /// <summary>
        /// Callback handler: Logs salary modification to console.
        /// Implements SalaryModificationCallback delegate.
        /// </summary>
        public static void LogSalaryModificationToConsole(
            Employee employee,
            decimal previousSalary,
            decimal newSalary,
            DateTime modificationTime)
        {
            var change = newSalary - previousSalary;
            var percentChange = (change / previousSalary) * 100;

            Console.WriteLine(
                $"💰 Salary Modified: {employee.FirstName} {employee.LastName}\n" +
                $"   Previous: ${previousSalary:F2}\n" +
                $"   New:      ${newSalary:F2}\n" +
                $"   Change:   ${change:F2} ({percentChange:F2}%)\n" +
                $"   Time:     {modificationTime:yyyy-MM-dd HH:mm:ss}");
        }

        /// <summary>
        /// Callback handler: Logs salary modification to audit trail (simulated).
        /// Demonstrates audit compliance use case.
        /// </summary>
        public static void LogSalaryModificationToAuditTrail(
            Employee employee,
            decimal previousSalary,
            decimal newSalary,
            DateTime modificationTime)
        {
            var auditEntry = new
            {
                Timestamp = modificationTime,
                EmployeeId = employee.AssociateId,
                EmployeeName = $"{employee.FirstName} {employee.LastName}",
                OldSalary = previousSalary,
                NewSalary = newSalary,
                Change = newSalary - previousSalary,
                AuditLevel = "HIGH" // Sensitive operation
            };

            Console.WriteLine(
                $"[AUDIT TRAIL] {auditEntry.Timestamp:O} | " +
                $"Employee: {auditEntry.EmployeeId} | " +
                $"Salary: ${auditEntry.OldSalary} → ${auditEntry.NewSalary}");
        }

        /// <summary>
        /// Callback handler: Validates salary change before logging.
        /// Demonstrates callbacks that can reject or modify behavior.
        /// </summary>
        public static void ValidateSalaryModification(
            Employee employee,
            decimal previousSalary,
            decimal newSalary,
            DateTime modificationTime)
        {
            if (newSalary < 0)
            {
                Console.WriteLine("❌ Invalid: Salary cannot be negative");
                return;
            }

            var percentChange = ((newSalary - previousSalary) / previousSalary) * 100;
            if (percentChange > 50)
            {
                Console.WriteLine(
                    $"⚠️  Warning: Large salary increase ({percentChange:F2}%) for {employee.FirstName}");
            }
        }

        // ========== ERROR HANDLING CALLBACKS ==========

        /// <summary>
        /// Callback handler: Logs processing errors to console.
        /// Implements ProcessingErrorCallback delegate.
        /// </summary>
        public static void LogProcessingErrorToConsole(
            string employeeId,
            string operationName,
            Exception exception)
        {
            Console.WriteLine(
                $"❌ ERROR in {operationName}\n" +
                $"   Employee ID: {employeeId}\n" +
                $"   Exception: {exception.Message}");
        }

        /// <summary>
        /// Callback handler: Logs processing errors to audit trail.
        /// Demonstrates audit trail for error tracking.
        /// </summary>
        public static void LogProcessingErrorToAuditTrail(
            string employeeId,
            string operationName,
            Exception exception)
        {
            Console.WriteLine(
                $"[AUDIT] ERROR | Operation: {operationName} | " +
                $"Employee: {employeeId} | " +
                $"Exception: {exception.GetType().Name} - {exception.Message}");
        }

        // ========== SALARY MODIFICATION WITH CALLBACKS ==========

        /// <summary>
        /// Demonstrates callback pattern: Modifies salary and notifies via callbacks.
        /// 
        /// KEY PATTERN: Operation logic + pluggable callbacks
        /// 
        /// Benefits:
        /// - Core logic independent of side effects
        /// - Caller controls what happens after operation
        /// - Easy to test by passing test callbacks
        /// - Enables logging, auditing, notifications without modifying code
        /// </summary>
        public static bool ModifyEmployeeSalary(
            Employee employee,
            decimal newSalary,
            SalaryModificationCallback? onSuccess = null,
            ProcessingErrorCallback? onError = null)
        {
            try
            {
                if (employee.EmployeeDetail == null)
                {
                    onError?.Invoke(
                        employee.AssociateId,
                        nameof(ModifyEmployeeSalary),
                        new InvalidOperationException("Employee has no details"));
                    return false;
                }

                var previousSalary = employee.EmployeeDetail.Salary;

                if (newSalary < 0)
                {
                    onError?.Invoke(
                        employee.AssociateId,
                        nameof(ModifyEmployeeSalary),
                        new ArgumentException("Salary cannot be negative"));
                    return false;
                }

                // Perform the actual modification
                employee.EmployeeDetail.Salary = newSalary;

                // Invoke success callbacks
                onSuccess?.Invoke(employee, previousSalary, newSalary, DateTime.Now);

                return true;
            }
            catch (Exception ex)
            {
                onError?.Invoke(
                    employee.AssociateId,
                    nameof(ModifyEmployeeSalary),
                    ex);
                return false;
            }
        }

        // ========== BATCH PROCESSING WITH PROGRESS CALLBACKS ==========

        /// <summary>
        /// Demonstrates progress callback pattern for batch operations.
        /// 
        /// USE CASE: Long-running operations needing progress updates
        /// Benefits:
        /// - Real-time progress feedback
        /// - Ability to show progress bars / update UI
        /// - Flexibility in how progress is reported
        /// </summary>
        public static void ProcessEmployeeBatch(
            List<Employee> employees,
            Func<Employee, bool> processingLogic,
            ProcessingProgressCallback? onProgress = null,
            BatchProcessingCallback? onComplete = null)
        {
            var startTime = DateTime.Now;
            int successCount = 0;
            int failCount = 0;
            var failedIds = new List<string>();

            for (int i = 0; i < employees.Count; i++)
            {
                var employee = employees[i];

                try
                {
                    // Execute processing logic
                    if (processingLogic(employee))
                    {
                        successCount++;
                    }
                    else
                    {
                        failCount++;
                        failedIds.Add(employee.AssociateId);
                    }
                }
                catch
                {
                    failCount++;
                    failedIds.Add(employee.AssociateId);
                }

                // Report progress
                onProgress?.Invoke(i + 1, employees.Count, employee.AssociateId);
            }

            // Report completion
            var result = new BatchProcessingResult(
                employees.Count,
                successCount,
                failCount,
                failedIds,
                DateTime.Now - startTime);

            onComplete?.Invoke(result);
        }

        // ========== ASYNC CALLBACK PATTERN ==========

        /// <summary>
        /// Async version of salary modification callback.
        /// Demonstrates delegates work seamlessly with async/await.
        /// 
        /// USE CASE: I/O-bound operations (database, API) as callbacks
        /// Benefits:
        /// - Non-blocking audit logging
        /// - Scalable notification system
        /// - Async all the way down
        /// </summary>
        public delegate Task AsyncSalaryModificationCallback(
            Employee employee,
            decimal previousSalary,
            decimal newSalary,
            DateTime modificationTime);

        /// <summary>
        /// Async callback handler: Simulates writing to database.
        /// </summary>
        public static async Task LogSalaryModificationAsync(
            Employee employee,
            decimal previousSalary,
            decimal newSalary,
            DateTime modificationTime)
        {
            // Simulate async database write
            await Task.Delay(100);
            Console.WriteLine($"[ASYNC DB] Logged salary change for {employee.AssociateId}");
        }

        /// <summary>
        /// Async version: Modifies salary with async callbacks.
        /// </summary>
        public static async Task<bool> ModifyEmployeeSalaryAsync(
            Employee employee,
            decimal newSalary,
            AsyncSalaryModificationCallback? onSuccess = null,
            ProcessingErrorCallback? onError = null)
        {
            try
            {
                if (employee.EmployeeDetail == null)
                {
                    onError?.Invoke(
                        employee.AssociateId,
                        nameof(ModifyEmployeeSalaryAsync),
                        new InvalidOperationException("Employee has no details"));
                    return false;
                }

                var previousSalary = employee.EmployeeDetail.Salary;

                // Simulate processing delay
                await Task.Delay(50);

                employee.EmployeeDetail.Salary = newSalary;

                // Await async callbacks
                if (onSuccess != null)
                {
                    await onSuccess(employee, previousSalary, newSalary, DateTime.Now);
                }

                return true;
            }
            catch (Exception ex)
            {
                onError?.Invoke(
                    employee.AssociateId,
                    nameof(ModifyEmployeeSalaryAsync),
                    ex);
                return false;
            }
        }

        // ========== PRACTICAL DEMONSTRATION ==========

        /// <summary>
        /// Demonstrates combining multiple callbacks for comprehensive processing.
        /// 
        /// Pattern: Single operation, multiple observers
        /// - Log to console
        /// - Log to audit trail
        /// - Validate change
        /// 
        /// Shows how delegates enable separation of concerns.
        /// </summary>
        public static void DemoMultipleCallbacks()
        {
            var employee = new Employee
            {
                AssociateId = "EMP001",
                FirstName = "John",
                LastName = "Doe",
                EmployeeDetail = new EmployeeDetail { Salary = 50000 }
            };

            Console.WriteLine("=== Salary Modification with Multiple Callbacks ===\n");

            // Compose multiple callbacks - assign to delegate first before using + operator
            SalaryModificationCallback? composedCallback = LogSalaryModificationToConsole;
            composedCallback += LogSalaryModificationToAuditTrail;
            composedCallback += ValidateSalaryModification;

            // Single operation, multiple handlers execute
            var success = ModifyEmployeeSalary(
                employee,
                55000,
                onSuccess: composedCallback,
                onError: LogProcessingErrorToConsole);

            Console.WriteLine($"\nOperation Success: {success}");
        }

        /// <summary>
        /// Demonstrates conditional callback composition.
        /// 
        /// Use Case: Different audit levels based on salary change magnitude
        /// Shows: Runtime decision making about which callbacks to use
        /// </summary>
        public static void DemoConditionalCallbacks()
        {
            var employee = new Employee
            {
                AssociateId = "EMP002",
                FirstName = "Jane",
                LastName = "Smith",
                EmployeeDetail = new EmployeeDetail { Salary = 60000 }
            };

            decimal newSalary = 75000;
            var salaryChange = newSalary - employee.EmployeeDetail.Salary;
            var percentChange = (salaryChange / employee.EmployeeDetail.Salary) * 100;

            Console.WriteLine("=== Conditional Callback Composition ===\n");

            // Choose callbacks based on change magnitude
            SalaryModificationCallback? callback = LogSalaryModificationToConsole;

            if (percentChange > 20)
            {
                // Large change requires audit trail
                callback += LogSalaryModificationToAuditTrail;
                callback += ValidateSalaryModification;
                Console.WriteLine("⚠️  Large salary change detected - Full audit enabled\n");
            }

            ModifyEmployeeSalary(
                employee,
                newSalary,
                onSuccess: callback);
        }
    }
}
