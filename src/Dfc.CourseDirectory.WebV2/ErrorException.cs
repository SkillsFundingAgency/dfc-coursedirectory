using System;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2
{
    public class ErrorException<T> : Exception
    {
        public ErrorException(T reason)
            : this(new Error<T>(reason))
        {
        }

        public ErrorException(Error<T> error)
        {
            Error = error;
        }

        public Error<T> Error { get; }

        public override string ToString() => Error.Value.ToString();
    }
}
