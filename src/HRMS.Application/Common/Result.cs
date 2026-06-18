namespace HRMS.Application.Common;

public enum ErrorSeverity 
{ 
    Error, 
    Warning, 
    Info, 
    Validation, 
    NotFound 
}

public sealed record Error(string Code, string Message, ErrorSeverity Severity = ErrorSeverity.Error);

public class Result
{
    public bool IsSuccess { get; }
    public Error? Error { get; }
    public IReadOnlyList<Error> Errors { get; }
    public bool IsFailure => !IsSuccess;

    protected Result(bool isSuccess, Error? error, IReadOnlyList<Error>? errors)
    {
        IsSuccess = isSuccess;
        Error = error;
        Errors = errors ?? Array.Empty<Error>();
    }

    public static Result Success() => new(true, null, null);
    public static Result Failure(Error error) => new(false, error, [error]);
    public static Result Failure(IReadOnlyList<Error> errors) => new(false, errors.FirstOrDefault(), errors);
    public static Result NotFound(string message) => Failure(new Error("NotFound", message, ErrorSeverity.NotFound));
    public static Result ValidationError(IReadOnlyList<Error> errors) => new(false, errors.FirstOrDefault(), errors);
}

public class Result<T> : Result
{
    public T? Value { get; }

    private Result(T? value, bool isSuccess, Error? error, IReadOnlyList<Error>? errors) 
        : base(isSuccess, error, errors)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new(value, true, null, null);
    public static new Result<T> Failure(Error error) => new(default, false, error, [error]);
    public static new Result<T> Failure(IReadOnlyList<Error> errors) => new(default, false, errors.FirstOrDefault(), errors);
    public static new Result<T> NotFound(string message) => Failure(new Error("NotFound", message, ErrorSeverity.NotFound));
    public static new Result<T> ValidationError(IReadOnlyList<Error> errors) => new(default, false, errors.FirstOrDefault(), errors);

    public static implicit operator Result<T>(T value) => Success(value);
}
