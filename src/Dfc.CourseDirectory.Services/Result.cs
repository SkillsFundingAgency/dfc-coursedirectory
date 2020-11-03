using System;

namespace Dfc.CourseDirectory.Services
{
    public struct Result
    {
        private static readonly Result OkResult = new Result(true, null);

        public bool IsSuccess { get; }
        public string Error { get; }

        private Result(bool isSuccess, string error)
        {
            if (!isSuccess && string.IsNullOrWhiteSpace(error))
            {
                throw new ArgumentException($"For a failure {nameof(error)} cannot be null, empty or only whitespace.");
            }

            IsSuccess = isSuccess;
            Error = error;
        }

        public static Result Ok()
        {
            return OkResult;
        }

        public static Result Fail(string error)
        {
            return new Result(false, error);
        }

        public static Result<T> Ok<T>(T value)
        {
            return Result<T>.Ok(value);
        }

        public static Result<T> Fail<T>(string error)
        {
            return Result<T>.Fail(error);
        }
    }

    public struct Result<T>
    {
        public bool IsSuccess { get; }
        public T Value { get; }
        public string Error { get; }

        private Result(bool isSuccess, T value, string error)
        {
            if (!isSuccess && string.IsNullOrWhiteSpace(error))
            {
                throw new ArgumentException($"For a failure {nameof(error)} cannot be null, empty or only whitespace.");
            }
                
            if (isSuccess && value == null)
            {
                throw new ArgumentException($"No failure therefore {nameof(value)} cannot be null.");
            }

            IsSuccess = isSuccess;
            Value = value;
            Error = error;
        }

        public static Result<T> Ok(T value)
        {
            return new Result<T>(true, value, null);
        }

        public static Result<T> Fail(string error)
        {
            return new Result<T>(false, default, error);
        }
    }
}
