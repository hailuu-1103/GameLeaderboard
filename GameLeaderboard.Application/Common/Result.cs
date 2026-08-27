namespace GameLeaderboard.Application.Common;

public sealed class Result<T>
{
    private Result(T value)
    {
        this.Value     = value;
        this.IsSuccess = true;
    }

    private Result(ApplicationError error)
    {
        this.Error     = error;
        this.IsSuccess = false;
    }

    public bool IsSuccess { get; }

    public T Value =>
        this.IsSuccess
            ? field!
            : throw new InvalidOperationException(
                "A failed result has no value.");

    public ApplicationError Error =>
        !this.IsSuccess
            ? field!
            : throw new InvalidOperationException(
                "A successful result has no error.");

    public static Result<T> Success(T value)
    {
        return new(value);
    }

    public static Result<T> Failure(
        ApplicationError error)
    {
        return new(error);
    }
}