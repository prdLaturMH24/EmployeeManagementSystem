using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.Utilities.Delegates
{
    /// <summary>
    /// Demonstrates C# Action<> Delegate Type and Multicast Delegates
    /// 
    /// USE CASE: Employee Event Notification System
    /// =============================================
    /// Action<> is used when you need to:
    /// 1. Perform actions without returning a value
    /// 2. Chain multiple handlers together (multicast delegates)
    /// 3. Implement observer pattern / event-like behavior
    /// 4. Decouple event publishers from subscribers
    /// 
    /// KEY CONCEPTS:
    /// - Action<T>: Generic delegate for operations without return value
    /// - Multicast Delegate: Single delegate variable holding multiple methods
    /// - += operator: Adds handler to delegate chain
    /// - -= operator: Removes handler from delegate chain
    /// - Null-coalescing check: Verify delegate is not null before invoking
    /// 
    /// ADVANTAGE OVER EVENTS: Direct access to delegate = simpler testing/demo
    /// (Events use delegates internally with controlled exposure)
    /// </summary>
    public class EmployeeNotificationDelegate
    {
        /// <summary>
        /// CUSTOM DELEGATE using Action pattern.
        /// Defines what information is passed when an employee event occurs.
        /// 
        /// Use Case: Consistent notification format across all subscribers
        /// Benefits: Domain-specific type with clear intent
        /// </summary>
        public delegate void EmployeeEventHandler(
            string eventType,
            Employee employee,
            DateTime eventDateTime);

        /// <summary>
        /// CUSTOM DELEGATE for detailed notifications with metadata.
        /// More complex than simple Action - carries rich context.
        /// </summary>
        public delegate void EmployeeAuditNotifier(
            EmployeeAuditEvent auditEvent);

        /// <summary>
        /// Represents an employee audit event with complete context.
        /// Used with EmployeeAuditNotifier delegate.
        /// </summary>
        public record EmployeeAuditEvent(
            string EventType,
            Employee Employee,
            DateTime EventDateTime,
            string PerformedBy,
            string Details);

        // ========== NOTIFICATION HANDLERS ==========

        /// <summary>
        /// Handler: Logs employee events to console.
        /// Matches Action<Employee> signature for use with delegates.
        /// 
        /// Demonstrates: Simple handler implementation
        /// </summary>
        public static void LogEmployeeEventToConsole(Employee employee, string eventType)
        {
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] " +
                            $"Employee Event: {eventType} - " +
                            $"{employee.FirstName} {employee.LastName} ({employee.AssociateId})");
        }

        /// <summary>
        /// Handler: Sends notification for employee additions.
        /// Demonstrates specific handler for single event type.
        /// </summary>
        public static void NotifyOnEmployeeAdded(Employee employee)
        {
            Console.WriteLine($"✓ New employee registered: {employee.FirstName} {employee.LastName}");
        }

        /// <summary>
        /// Handler: Sends notification for employee deletions.
        /// Demonstrates audit trail for sensitive operations.
        /// </summary>
        public static void NotifyOnEmployeeDeleted(Employee employee)
        {
            Console.WriteLine($"✗ Employee removed: {employee.FirstName} {employee.LastName} " +
                            $"(ID: {employee.AssociateId})");
        }

        /// <summary>
        /// Handler: Validates employee before processing.
        /// Demonstrates handler that can prevent operations (if needed).
        /// </summary>
        public static void ValidateBeforeProcessing(Employee employee)
        {
            if (string.IsNullOrWhiteSpace(employee.FirstName) ||
                string.IsNullOrWhiteSpace(employee.LastName))
            {
                Console.WriteLine("⚠ Warning: Employee has incomplete name");
            }
        }

        /// <summary>
        /// Handler: Records employee event in audit log (simulated).
        /// Demonstrates rich audit logging.
        /// </summary>
        public static void AuditLogEmployeeEvent(
            string eventType,
            Employee employee,
            DateTime eventDateTime)
        {
            var logEntry = $"[AUDIT] {eventDateTime:yyyy-MM-dd HH:mm:ss} | " +
                          $"Event: {eventType} | " +
                          $"Employee: {employee.AssociateId} | " +
                          $"Name: {employee.FirstName} {employee.LastName}";
            Console.WriteLine(logEntry);
        }

        /// <summary>
        /// Handler: Sends email notification (simulated).
        /// Demonstrates external system integration.
        /// </summary>
        public static void SendEmailNotification(
            string eventType,
            Employee employee,
            DateTime eventDateTime)
        {
            Console.WriteLine($"📧 Email sent: Employee {eventType} notification to admin@company.com");
        }

        // ========== NOTIFICATION MANAGER ==========

        /// <summary>
        /// Demonstrates multicast delegate pattern for notification system.
        /// 
        /// KEY PATTERN: Multiple handlers registered and invoked together
        /// Benefits:
        /// - Loose coupling between event source and handlers
        /// - Handlers can be added/removed at runtime
        /// - All handlers invoked regardless of exceptions (be careful!)
        /// - Perfect for logging, notifications, audit trails
        /// </summary>
        public class EmployeeNotificationManager
        {
            // Multicast delegates - can hold multiple methods
            // Using Action<> for operations without return value

            /// <summary>
            /// Multicast delegate for employee addition events.
            /// Multiple handlers can be attached to react to the same event.
            /// 
            /// Demonstrates: += to attach handlers, -= to detach
            /// </summary>
            public Action<Employee>? OnEmployeeAdded { get; set; }

            /// <summary>
            /// Multicast delegate for employee update events.
            /// </summary>
            public Action<Employee>? OnEmployeeUpdated { get; set; }

            /// <summary>
            /// Multicast delegate for employee deletion events.
            /// </summary>
            public Action<Employee>? OnEmployeeDeleted { get; set; }

            /// <summary>
            /// Multicast delegate using custom delegate type.
            /// Carries more context than simple Action<Employee>.
            /// </summary>
            public EmployeeEventHandler? OnEmployeeEvent { get; set; }

            /// <summary>
            /// Multicast delegate for audit events.
            /// </summary>
            public EmployeeAuditNotifier? OnAuditEvent { get; set; }

            /// <summary>
            /// Invokes employee added handlers.
            /// 
            /// Pattern:
            /// 1. Check if delegate is not null (it might have no subscribers)
            /// 2. Invoke the delegate - all attached handlers are called
            /// 3. Handlers execute in the order they were added
            /// 
            /// Best Practice: Use null-coalescing (?.) or null-check
            /// </summary>
            public void RaiseEmployeeAdded(Employee employee)
            {
                Console.WriteLine($"\n--- Employee Added Event ---");

                // Null-coalescing: Only invoke if delegate is not null
                OnEmployeeAdded?.Invoke(employee);
            }

            /// <summary>
            /// Invokes employee updated handlers.
            /// Demonstrates alternative invocation pattern.
            /// </summary>
            public void RaiseEmployeeUpdated(Employee employee)
            {
                Console.WriteLine($"\n--- Employee Updated Event ---");

                if (OnEmployeeUpdated != null)
                {
                    OnEmployeeUpdated(employee);
                }
            }

            /// <summary>
            /// Invokes employee deleted handlers.
            /// </summary>
            public void RaiseEmployeeDeleted(Employee employee)
            {
                Console.WriteLine($"\n--- Employee Deleted Event ---");
                OnEmployeeDeleted?.Invoke(employee);
            }

            /// <summary>
            /// Invokes employee event handlers with rich context.
            /// </summary>
            public void RaiseEmployeeEvent(
                string eventType,
                Employee employee)
            {
                OnEmployeeEvent?.Invoke(eventType, employee, DateTime.Now);
            }

            /// <summary>
            /// Invokes audit event handlers.
            /// </summary>
            public void RaiseAuditEvent(
                string eventType,
                Employee employee,
                string performedBy,
                string details)
            {
                OnAuditEvent?.Invoke(new EmployeeAuditEvent(
                    eventType,
                    employee,
                    DateTime.Now,
                    performedBy,
                    details));
            }

            /// <summary>
            /// Registers default notification handlers.
            /// Demonstrates populating multicast delegate with handlers.
            /// </summary>
            public void RegisterDefaultHandlers()
            {
                // Subscribe to OnEmployeeAdded
                OnEmployeeAdded += NotifyOnEmployeeAdded;
                OnEmployeeAdded += ValidateBeforeProcessing;

                // Subscribe to OnEmployeeDeleted
                OnEmployeeDeleted += NotifyOnEmployeeDeleted;

                // Subscribe to OnEmployeeEvent
                OnEmployeeEvent += AuditLogEmployeeEvent;
                OnEmployeeEvent += SendEmailNotification;
            }

            /// <summary>
            /// Unregisters all default handlers.
            /// Demonstrates -= operator for removing handlers.
            /// </summary>
            public void UnregisterDefaultHandlers()
            {
                OnEmployeeAdded -= NotifyOnEmployeeAdded;
                OnEmployeeAdded -= ValidateBeforeProcessing;

                OnEmployeeDeleted -= NotifyOnEmployeeDeleted;

                OnEmployeeEvent -= AuditLogEmployeeEvent;
                OnEmployeeEvent -= SendEmailNotification;
            }
        }

        // ========== LAMBDA-BASED HANDLERS ==========

        /// <summary>
        /// Demonstrates lambda expressions with delegates.
        /// 
        /// USE CASE: Quick, inline handler definitions without separate methods
        /// Benefits:
        /// - Concise syntax for simple operations
        /// - Closure over local variables
        /// - Direct integration with business logic
        /// </summary>
        public class LambdaNotificationExamples
        {
            /// <summary>
            /// Creates a notification manager with lambda-based handlers.
            /// Shows how lambdas can replace named methods for quick implementations.
            /// </summary>
            public static EmployeeNotificationManager CreateWithLambdaHandlers()
            {
                var manager = new EmployeeNotificationManager();

                // Lambda handler: Log to console
                manager.OnEmployeeAdded += emp =>
                    Console.WriteLine($"[Lambda] Added: {emp.FirstName} {emp.LastName}");

                // Lambda handler: Validate salary range
                manager.OnEmployeeAdded += emp =>
                {
                    if (emp.EmployeeDetail?.Salary > 0)
                        Console.WriteLine($"[Lambda] Salary OK: ${emp.EmployeeDetail.Salary}");
                    else
                        Console.WriteLine($"[Lambda] ⚠ Invalid salary for {emp.FirstName}");
                };

                // Lambda handler: Log deletion with timestamp
                manager.OnEmployeeDeleted += emp =>
                    Console.WriteLine($"[Lambda] {DateTime.Now:HH:mm:ss} - Removed: {emp.AssociateId}");

                return manager;
            }

            /// <summary>
            /// Demonstrates Func<T, bool> for conditional logic.
            /// 
            /// USE CASE: Create reusable predicates for filtering events
            /// Example: "Only process events for salaried employees"
            /// </summary>
            public static void ConditionalNotification()
            {
                // Predicate: Determines if event should be processed
                Func<Employee, bool> shouldNotify = emp =>
                    emp.EmployeeDetail != null &&
                    emp.EmployeeDetail.Salary >= 50000;

                var testEmployee = new Employee
                {
                    AssociateId = "EMP001",
                    FirstName = "John",
                    LastName = "Doe",
                    EmployeeDetail = new EmployeeDetail { Salary = 60000 }
                };

                if (shouldNotify(testEmployee))
                {
                    Console.WriteLine($"Notifying high-salary employee: {testEmployee.FirstName}");
                }
            }
        }

        // ========== PRACTICAL USAGE PATTERN ==========

        /// <summary>
        /// Demonstrates a simple publisher-subscriber pattern using delegates.
        /// 
        /// Pattern Flow:
        /// 1. Publisher (EmployeeNotificationManager) owns the delegate
        /// 2. Subscribers attach handlers via += operator
        /// 3. Publisher raises events by invoking delegates
        /// 4. All subscribers' handlers execute
        /// 
        /// Benefits: Decoupling, extensibility, testability
        /// </summary>
        public static void DemoNotificationSystem()
        {
            var manager = new EmployeeNotificationManager();

            // Subscribers register interest in events
            manager.OnEmployeeAdded += emp =>
                Console.WriteLine($"📢 Notification: Welcome {emp.FirstName}!");

            manager.OnEmployeeAdded += emp =>
                Console.WriteLine($"📝 Audit: {emp.AssociateId} added to system");

            manager.OnEmployeeAdded += emp =>
                Console.WriteLine($"✉️ Email sent to HR about new employee");

            // Create sample employee
            var employee = new Employee
            {
                AssociateId = "EMP001",
                FirstName = "Alice",
                LastName = "Johnson"
            };

            // Publisher raises event - all handlers execute
            manager.RaiseEmployeeAdded(employee);
        }
    }
}
