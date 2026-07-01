namespace Partners.Core.Results;

public class Result<T>
{
    public bool Success { get; init; }
    public T? Value { get; init; }
    public List<string> Errors { get; init; } = [];

    public static Result<T> Ok(T value) => new() { Success = true, Value = value };

    public static Result<T> Fail(params string[] errors) => new() { Success = false, Errors = [.. errors] };
}
