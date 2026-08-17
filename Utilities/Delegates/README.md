# C# Delegates in Employee Management System

This folder contains comprehensive examples of C# delegate patterns and their practical applications in an employee management system.

## Overview

Delegates are a fundamental C# feature that enables:
- **Loose Coupling**: Decouple event sources from handlers
- **Extensibility**: Add functionality without modifying existing code
- **Flexibility**: Pass behavior as parameters
- **Composition**: Combine multiple operations into pipelines

## Files

### 1. EmployeeValidationDelegate.cs
**Focus**: Custom Delegates, `Func<>`, and Validation Predicates

**Key Concepts**:
- **Custom Delegate Types**: Domain-specific delegates for clarity
- **Func<T, TResult>**: Generic delegates returning values (used for transformations)
- **Predicate<T>**: Boolean-returning delegates (essentially `Func<T, bool>`)
- **Validation Pipeline**: Composable validation rules

**Use Cases**:
1. **Custom Employee Validation**
   ```csharp
   public delegate (bool isValid, string errorMessage) EmployeeValidationRule(Employee emp);
   ```

2. **Reusable Validation Predicates**
   ```csharp
   var isComplete = emp => emp.FirstName != null && emp.EmployeeDetail != null;
   ```

3. **Validation Pipeline Pattern**
   ```csharp
   var pipeline = EmployeeValidationPipeline.CreateStandardPipeline();
   pipeline.AddValidationRule(ValidateEmployeeId);
   pipeline.AddValidationRule(ValidateEmployeeName);
   var (isValid, errors) = pipeline.ValidateEmployee(employee);
   ```

4. **LINQ Integration**
   ```csharp
   var validEmployees = employees.Where(ValidationPredicates.HasCompleteName).ToList();
   ```

### 2. EmployeeNotificationDelegate.cs
**Focus**: `Action<>`, Multicast Delegates, and Publisher-Subscriber Pattern

**Key Concepts**:
- **Action<T>**: Generic delegates for operations without return values
- **Multicast Delegates**: Single delegate variable holding multiple methods
- **Event-like Behavior**: Loose coupling via += and -= operators
- **Lambda Handlers**: Inline callback definitions

**Use Cases**:
1. **Custom Event Handlers**
   ```csharp
   public delegate void EmployeeEventHandler(string eventType, Employee emp, DateTime time);
   ```

2. **Multicast Delegate (Multiple Observers)**
   ```csharp
   manager.OnEmployeeAdded += NotifyOnEmployeeAdded;
   manager.OnEmployeeAdded += ValidateBeforeProcessing;
   manager.RaiseEmployeeAdded(employee);  // Both handlers execute
   ```

3. **Lambda-Based Handlers**
   ```csharp
   manager.OnEmployeeAdded += emp => 
	   Console.WriteLine($"Welcome {emp.FirstName}!");
   ```

4. **Publisher-Subscriber Pattern**
   - Handlers register interest in events via `+=`
   - Publishers raise events by invoking delegates
   - All subscribers' handlers execute in registration order

### 3. EmployeeProcessingDelegate.cs
**Focus**: Callback Pattern, Batch Processing, and Async Delegates

**Key Concepts**:
- **Callback Pattern**: Pass operations to execute after a task completes
- **Progress Reporting**: Track long-running operations
- **Error Handling**: Custom error strategies via callbacks
- **Async Delegates**: `Task` return type for I/O-bound callbacks
- **Conditional Composition**: Runtime selection of callbacks

**Use Cases**:
1. **Salary Modification with Callbacks**
   ```csharp
   ModifyEmployeeSalary(
	   employee,
	   55000,
	   onSuccess: LogSalaryModificationToConsole,
	   onError: LogProcessingErrorToConsole);
   ```

2. **Callback Composition**
   ```csharp
   var callbacks = LogSalaryModificationToConsole;
   callbacks += LogSalaryModificationToAuditTrail;
   callbacks += ValidateSalaryModification;

   ModifyEmployeeSalary(employee, 55000, callbacks);
   ```

3. **Batch Processing with Progress**
   ```csharp
   ProcessEmployeeBatch(
	   employees,
	   emp => emp.EmployeeDetail.Salary > 0,
	   onProgress: (processed, total, id) => 
		   Console.WriteLine($"Processed {processed}/{total}"),
	   onComplete: result => 
		   Console.WriteLine($"Completed: {result.SuccessfulCount}/{result.TotalProcessed}"));
   ```

4. **Async Callbacks**
   ```csharp
   await ModifyEmployeeSalaryAsync(
	   employee,
	   55000,
	   onSuccess: LogSalaryModificationAsync);
   ```

## Delegate Types Reference

| Type | Return | Use Case | Example |
|------|--------|----------|---------|
| `Action<T>` | void | Operations without return | Logging, notifications |
| `Func<T, TResult>` | TResult | Transformations, conditions | Filtering, mapping |
| `Predicate<T>` | bool | Filtering criteria | LINQ Where clauses |
| `Custom Delegate` | Any | Domain-specific types | Rich context payloads |

## EmployeeService Integration

The `EmployeeService` class demonstrates practical delegate usage:

```csharp
// Flexible filtering
var managers = await service.FilterEmployeesWithDetailsAsync(
	e => e.EmployeeDetail?.Position == "Manager");

// Validation pipeline
var (success, errors) = await service.ValidateAndAddEmployeeAsync(employeeDto, pipeline);

// Custom transformation
var names = await service.TransformEmployeesAsync(e => e.FirstName + " " + e.LastName);

// Side effects
await service.ForEachEmployeeAsync(e => Console.WriteLine(e.FirstName));

// Complex criteria
var results = await service.GetEmployeesByCriteriaAsync(
	e => e.EmployeeDetail != null,
	e => e.EmployeeDetail.Salary > 50000,
	e => e.FirstName.Length > 0);
```

## Key Patterns Demonstrated

### 1. **Validation Pipeline**
Decouple validation rules from business logic:
```csharp
pipeline.AddValidationRule(ValidateEmployeeId);
pipeline.AddValidationRule(ValidateEmployeeName);
// Add more rules without modifying AddEmployeeAsync
```

### 2. **Publisher-Subscriber**
Loose coupling between event sources and handlers:
```csharp
manager.OnEmployeeAdded += Handler1;
manager.OnEmployeeAdded += Handler2;
manager.RaiseEmployeeAdded(employee);  // Both handlers execute
```

### 3. **Callback Pattern**
Execute post-operation logic:
```csharp
ModifyEmployeeSalary(employee, newSalary, onSuccess: LogToAudit, onError: HandleError);
```

### 4. **Filtering Predicates**
Flexible query criteria:
```csharp
var results = employees.Where(validationRule).ToList();
```

### 5. **Conditional Composition**
Runtime selection of handlers:
```csharp
SalaryModificationCallback? callback = LogToConsole;
if (largeChange)
	callback += LogToAuditTrail;  // Only add for large changes
```

## Best Practices

### 1. **Null-Coalescing Before Invocation**
```csharp
// Good: Check before invoking
OnEmployeeAdded?.Invoke(employee);

// Avoid: Direct invocation without null check
// OnEmployeeAdded(employee);  // NullReferenceException risk
```

### 2. **Meaningful Delegate Names**
```csharp
// Good: Domain-specific name
public delegate void EmployeeEventHandler(...);

// Consider: When to use vs built-in Action/Func
// Use custom if: Rich context, domain clarity, reuse across files
// Use Action/Func if: Simple, single-file scope
```

### 3. **Immutability for Reusable Predicates**
```csharp
public static readonly Func<Employee, bool> HasValidEmail = emp => 
	emp.EmployeeDetail?.Email.Contains("@") ?? false;
```

### 4. **Async All the Way**
```csharp
// For async operations, use async delegates
public delegate Task AsyncSalaryModificationCallback(...);
await onSuccess?.Invoke(...);  // Not: onSuccess.Invoke(...).Wait()
```

### 5. **Clear Documentation**
Each delegate should document:
- **What**: What does this delegate represent?
- **When**: When is it invoked?
- **Why**: Why use delegate instead of direct method call?
- **Example**: How to use it?

## Performance Considerations

1. **Multicast Delegate Exceptions**: If one handler throws, subsequent handlers don't execute
2. **Delegate Invocation Overhead**: Minimal, but measure in hot paths
3. **GC Pressure**: Avoid creating delegates in tight loops; cache or reuse
4. **Async Deadlocks**: Always await async delegates; avoid `.Result` or `.Wait()`

## Testing Delegates

```csharp
[Fact]
public void TestValidationDelegate()
{
	var errors = new List<string>();

	// Mock error callback
	EmployeeValidationDelegate.ValidationResultCallback mockCallback = 
		(emp, isValid, msg) => { if (!isValid) errors.Add(msg); };

	var pipeline = new EmployeeValidationPipeline();
	pipeline.AddValidationRule(EmployeeValidationDelegate.ValidateEmployeeId);
	pipeline.RegisterValidationCallback(mockCallback);

	var invalidEmployee = new Employee { AssociateId = "" };
	var (valid, errs) = pipeline.ValidateEmployee(invalidEmployee);

	Assert.False(valid);
	Assert.NotEmpty(errors);
}
```

## Further Reading

- [Microsoft Learn: Delegates](https://learn.microsoft.com/dotnet/csharp/programming-guide/delegates/)
- [Microsoft Learn: Events](https://learn.microsoft.com/dotnet/csharp/events/)
- [Microsoft Learn: LINQ to Objects](https://learn.microsoft.com/dotnet/csharp/linq/)
- [Async/Await Best Practices](https://learn.microsoft.com/archive/msdn-magazine/2013/march/async-await-best-practices-in-asynchronous-programming)

## Summary

This implementation demonstrates that delegates are not just for events—they enable:
- **Flexible validation** without code modification
- **Loose coupling** via callbacks and notifications
- **Reusable transformations** and predicates
- **Composable operations** for complex workflows
- **Type-safe function passing** better than function pointers

By mastering delegates, you unlock powerful patterns for building extensible, maintainable C# applications.
