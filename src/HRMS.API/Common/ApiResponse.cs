namespace HRMS.API.Common;

public sealed record ApiResponse<T>(bool Success, T? Data, string? Message, IEnumerable<string>? Errors)
{
    public static ApiResponse<T> Ok(T data, string? message = null) => 
        new(true, data, message, null);

    public static ApiResponse<T> Fail(string message, IEnumerable<string>? errors = null) => 
        new(false, default, message, errors);
}

public sealed record ApiResponse(bool Success, string? Message, IEnumerable<string>? Errors)
{
    public static ApiResponse Ok(string? message = null) => 
        new(true, message, null);

    public static ApiResponse Fail(string message, IEnumerable<string>? errors = null) => 
        new(false, message, errors);
}
