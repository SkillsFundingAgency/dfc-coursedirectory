using System;

namespace Dfc.CourseDirectory.Core.Models
{
    [Flags]
    public enum ApprenticeshipQAStatus
    {
        NotStarted = 1,
        Submitted = 2,
        InProgress = 4,
        Failed = 8,
        Passed = 99,
        UnableToComplete = 32
    }

    public static class ApprenticeshipQAStatusExtensions
    {
        public static string ToDescription(this ApprenticeshipQAStatus status)
        {
            if (status.HasFlag(ApprenticeshipQAStatus.UnableToComplete))
            {
                return "Unable to complete";
            }

            switch (status)
            {
                case ApprenticeshipQAStatus.NotStarted:
                    return "New provider";
                case ApprenticeshipQAStatus.Submitted:
                    return "Ready for QA";
                case ApprenticeshipQAStatus.InProgress:
                    return "In progress";
                case ApprenticeshipQAStatus.Failed:
                    return "Failed";
                case ApprenticeshipQAStatus.Passed:
                    return "Passed";
                default:
                    throw new NotImplementedException($"Unknown value: '{status}'.");
            }
        }

        public static ApprenticeshipQAStatus ValueOrDefault(this ApprenticeshipQAStatus? status) =>
            status ?? ApprenticeshipQAStatus.NotStarted;
    }
}
