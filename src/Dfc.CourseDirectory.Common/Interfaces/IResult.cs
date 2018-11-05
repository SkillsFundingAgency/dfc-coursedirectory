namespace Dfc.CourseDirectory.Common.Interfaces
{
    public interface IResult
    {
        string Error { get; }
        bool IsSuccess { get; }
        bool IsFailure { get; }
    }

    public interface IResult<T> : IResult
    {
        T Value { get; }
        bool HasValue { get; }
    }
}