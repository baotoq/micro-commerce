namespace MicroCommerce.Catalog.Domain.Common;

/// <summary>
/// Lightweight discriminated-union-ish Result type. Used to model business-rule outcomes
/// (e.g. PRODUCT_SKU_DUPLICATE) without throwing exceptions for control flow.
/// </summary>
public readonly record struct ResultError(string Code, string Message);

public readonly record struct Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public ResultError? Error { get; }

    public bool IsFailure => !IsSuccess;

    private Result(bool isSuccess, T? value, ResultError? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static Result<T> Success(T value) => new(true, value, null);

    public static Result<T> Conflict(string code, string message) =>
        new(false, default, new ResultError(code, message));
}
