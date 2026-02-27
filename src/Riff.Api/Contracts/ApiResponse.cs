namespace Riff.Api.Contracts;

public sealed record ApiResponse<T>(
    bool Success,
    T? Data,
    ApiError? Error
);

public sealed record ApiError(
    string Code,
    IReadOnlyDictionary<string, string[]>? Fields = null
);

public static class ApiResponses
{
    public static ApiResponse<object?> Success() =>
        new(
            Success: true,
            Data: null,
            Error: null
        );

    public static ApiResponse<T> Success<T>(T data) =>
        new(
            Success: true,
            Data: data,
            Error: null
        );

    public static ApiResponse<object> Error(
        string code,
        IReadOnlyDictionary<string, string[]>? fields = null
    ) =>
        new(
            Success: false,
            Data: null,
            Error: new ApiError(
                Code: code,
                Fields: fields
            )
        );
}
