namespace BarbApp.Domain.Common;

/// <summary>
/// Resultado de uma operação que pode falhar
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public bool IsFailure => !IsSuccess;

    protected Result(bool isSuccess, string? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, null);

    public static Result Failure(string error) => new(false, error);
}

/// <summary>
/// Resultado de uma operação que pode falhar com valor de retorno
/// </summary>
public class Result<T> : Result
{
    public T? Data { get; }

    private Result(bool isSuccess, T? data, string? error)
        : base(isSuccess, error)
    {
        Data = data;
    }

    public static Result<T> Success(T data) => new(true, data, null);

    public static new Result<T> Failure(string error) => new(false, default, error);

    public static implicit operator Result<T>(T data) => Success(data);
}