namespace Resto.Api.Common;

// Thrown by services. GlobalExceptionHandler turns them into clean JSON errors,
// so controllers don't need try/catch everywhere.

/// <summary>The requested record does not exist. Becomes HTTP 404.</summary>
public class NotFoundException(string message) : Exception(message);

/// <summary>The request is valid, but breaks a business rule
/// (e.g. ordering an unavailable item). Becomes HTTP 422.</summary>
public class BusinessRuleException(string message) : Exception(message);
