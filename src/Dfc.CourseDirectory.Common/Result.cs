using Dfc.CourseDirectory.Common.Interfaces;
using System;

namespace Dfc.CourseDirectory.Common
{
    public struct Result : IResult
    {
        private static readonly Result OkResult = new Result(false, null);

        public string Error { get; }
        public bool IsSuccess => !IsFailure;
        public bool IsFailure { get; }

        private Result(bool isFailure, string error)
        {
            if (isFailure && string.IsNullOrWhiteSpace(error))
                throw new ArgumentException($"For a failure {nameof(error)} cannot be null, empty or only whitespace.");

            IsFailure = isFailure;
            Error = error;
        }

        public static Result Ok()
        {
            return OkResult;
        }

        public static Result Fail(string error)
        {
            return new Result(true, error);
        }

        public static Result<T> Ok<T>(T value)
        {
            return new Result<T>(false, value, null);
        }

        public static Result<T> Fail<T>(string error)
        {
            return new Result<T>(true, default(T), error);
        }
    }

    public struct Result<T> : IResult<T>
    {
        public T Value { get; }
        public bool HasValue => Value != null;
        public string Error { get; }
        public bool IsSuccess => !IsFailure;
        public bool IsFailure { get; }

        internal Result(bool isFailure, T value, string error)
        {
            if (isFailure && string.IsNullOrWhiteSpace(error))
                throw new ArgumentException($"For a failure {nameof(error)} cannot be null, empty or only whitespace.");
            if (!isFailure && value == null)
                throw new ArgumentException($"No failure therefore {nameof(value)} cannot be null.");

            IsFailure = isFailure;
            Error = error;
            Value = value;
        }
    }
}