using System;

namespace Dfc.CourseDirectory.WebV2
{
    public class InvalidStateException : Exception
    {
        public InvalidStateException()
            : this(InvalidStateReason.Unspecified)
        {
        }

        public InvalidStateException(InvalidStateReason reason)
        {
            Reason = reason;
        }

        public InvalidStateReason Reason { get; }

        public override string ToString() => Reason.ToString();
    }

    public enum InvalidStateReason
    {
        Unspecified = 0,
        ProviderDoesNotExist,
        ApprenticeshipDoesNotExist,
        InvalidProviderType,
        InvalidApprenticeshipQAStatus,
        InvalidApprenticeshipQASubmission,
        NoValidApprenticeshipQASubmission
    }
}
