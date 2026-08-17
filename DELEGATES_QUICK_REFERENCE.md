# Quick Reference: C# Delegates in Employee Management System

## 🎯 One-Minute Overview

Delegates are **type-safe function pointers** that enable:
- Passing methods as parameters
- Creating flexible pipelines
- Building event-driven systems
- Loose coupling between components

## 📚 Delegate Types at a Glance

### Custom Delegates (Domain-Specific)
```csharp
// Define once, use many times
public delegate (bool, string) EmployeeValidationRule(Employee emp);
public delegate void EmployeeEventHandler(string eventType, Employee emp, DateTime time);
```

### Generic Delegates (Built-in)
```csharp
Action<T>           // void operation(T item)
Func<T, TResult>    // TResult transform(T item)
Predicate<T>        // bool isMatch(T item) — just Func<T, bool>
```

## 💻 Quick Code Patterns

### 1️⃣ Filtering (Predicate)
```csharp
// Filter using Func<Employee, bool>
var results = await service.FilterEmployeesAsync(
	e => e.EmployeeDetail?.Salary > 50000);
```

### 2️⃣ Validation Pipeline
```csharp
var pipeline = EmployeeValidationPipeline.CreateStandardPipeline();
pipeline.AddValidationRule(ValidateEmployeeId);
var (isValid, errors) = pipeline.ValidateEmployee(emp);
```

### 3️⃣ Event Notifications (Action)
```csharp
var manager = new EmployeeNotificationManager();
manager.OnEmployeeAdded += emp => Console.WriteLine($"Added: {emp.FirstName}");
manager.OnEmployeeAdded += LogToAudit;  // Multiple handlers
manager.RaiseEmployeeAdded(employee);   // All handlers execute
```

### 4️⃣ Callbacks (Post-Operation)
```csharp
ModifyEmployeeSalary(
	employee,
	55000,
	onSuccess: LogSalaryChange,
	onError: LogError);
```

### 5️⃣ Transformation (Func)
```csharp
var names = await service.TransformEmployeesAsync(
	e => $"{e.FirstName} {e.LastName}");
```

### 6️⃣ Side Effects (Action)
```csharp
await service.ForEachEmployeeAsync(emp =>
	Console.WriteLine($"Processing: {emp.FirstName}"));
```

## 🔄 Delegate Operators

```csharp
myDelegate += newHandler;    // Attach handler (multicast)
myDelegate -= oldHandler;    // Detach handler
myDelegate?.Invoke(args);    // Invoke (null-safe)
myDelegate(args);            // Invoke (direct)
```

## 📋 When to Use Each Delegate Type

| Need | Use | Example |
|------|-----|---------|
| Filtering/Condition | `Func<T, bool>` | `e => e.Salary > 50000` |
| Transformation | `Func<T, TResult>` | `e => e.FirstName + " " + e.LastName` |
| Side Effect | `Action<T>` | `e => LogEmployee(e)` |
| Validation | Custom Delegate | `(bool, string)` for rich feedback |
| Event Handler | Custom/Action | Multiple subscribers |
| Callback | Custom Delegate | Post-operation logic |

## 🚀 Common Patterns

### Composing Handlers
```csharp
OnEmployeeAdded += Handler1;
OnEmployeeAdded += Handler2;
OnEmployeeAdded += Handler3;
// All three execute when event raised
```

### Conditional Composition
```csharp
Action<Employee>? handlers = LogToConsole;
if (needsAudit)
	handlers += LogToAuditTrail;
if (needsNotification)
	handlers += SendEmail;
// Execute composed handlers
```

### Pipeline Pattern
```csharp
pipeline.AddRule(Rule1);
pipeline.AddRule(Rule2);
pipeline.AddRule(Rule3);
var result = pipeline.Validate(employee);
```

## ⚠️ Common Gotchas

### ❌ Forgetting Null Check
```csharp
// WRONG: Might throw NullReferenceException
OnEmployeeAdded(employee);

// RIGHT: Null-coalescing
OnEmployeeAdded?.Invoke(employee);
```

### ❌ Using + Without Assignment
```csharp
// WRONG: Cannot compose method groups directly
SalaryModificationCallback composed = LogToConsole + LogToAuditTrail;

// RIGHT: Assign first, then use +=
SalaryModificationCallback? composed = LogToConsole;
composed += LogToAuditTrail;
```

### ❌ Forgetting Await
```csharp
// WRONG: Async delegate not awaited
await ModifyEmployeeSalaryAsync(emp, 55000, onSuccess: LogAsync);

// RIGHT: Await inside callback
await ModifyEmployeeSalaryAsync(emp, 55000, 
	onSuccess: async (e, old, new, time) => await LogAsync(e, old, new, time));
```

## 📂 File Structure

```
Utilities/
├── Delegates/
│   ├── EmployeeValidationDelegate.cs    (Validation + Predicates)
│   ├── EmployeeNotificationDelegate.cs  (Events + Multicast)
│   ├── EmployeeProcessingDelegate.cs    (Callbacks + Async)
│   └── README.md                        (Detailed Guide)
```

## 🧪 Testing Delegates

```csharp
[Fact]
public void TestValidation()
{
	var errors = new List<string>();

	// Mock callback
	Action<string> logError = msg => errors.Add(msg);

	// Test with mock
	var result = ValidateEmployee(invalidEmployee, logError);

	Assert.False(result.isValid);
	Assert.NotEmpty(errors);
}
```

## 📖 File Quick Links

- **EmployeeValidationDelegate.cs**: Custom delegates + Func<> + Predicates
- **EmployeeNotificationDelegate.cs**: Action<> + Multicast + Events
- **EmployeeProcessingDelegate.cs**: Callbacks + Async + Batch
- **EmployeeService.cs**: Integration examples (8 methods)
- **README.md**: Comprehensive guide with patterns
- **DELEGATES_IMPLEMENTATION_SUMMARY.md**: Full overview

## ✅ Checklist: Do I Need Delegates?

✓ Passing behavior as a parameter?  
✓ Multiple handlers for same event?  
✓ Dynamic filtering/validation criteria?  
✓ Decoupling event source from handlers?  
✓ Avoiding code duplication in similar methods?  

**If yes to any**: Delegates can help!

## 🎓 Key Takeaways

1. **Delegates are types**: They represent methods with specific signatures
2. **Custom vs Generic**: Use custom for domain clarity, generic for simplicity
3. **Multicast**: += to attach multiple handlers to one delegate
4. **Null-safe**: Always use `?.Invoke()` or check before invoking
5. **Async-ready**: Works seamlessly with `Task`/`async`/`await`
6. **LINQ-friendly**: Predicates work directly with `.Where()`, `.Select()`, etc.
7. **Decoupling**: Enables loose coupling and extensibility

## 🔗 Learn More

See **README.md** in Utilities/Delegates/ for comprehensive guide with:
- Detailed patterns and examples
- Performance considerations
- Best practices and anti-patterns
- Testing strategies
