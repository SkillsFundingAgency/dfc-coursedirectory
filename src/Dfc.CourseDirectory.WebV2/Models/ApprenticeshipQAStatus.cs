using System;

namespace Dfc.CourseDirectory.WebV2.Models
{
    public enum ApprenticeshipQAStatus
    {
        NotStarted = 0,
        Submitted = 1,
        InProgress = 2,
        Failed = 3,
        Passed = 4,
        UnableToComplete = 5
    }

    public static class ApprenticeshipQAStatusExtensions
    {
        public static string ToDisplayName(this ApprenticeshipQAStatus status)
        {
            switch (status)
            {
                case ApprenticeshipQAStatus.NotStarted:
                    return "Not started";
                case ApprenticeshipQAStatus.Submitted:
                    return "Ready for QA";
                case ApprenticeshipQAStatus.InProgress:
                    return "In progress";
                case ApprenticeshipQAStatus.Failed:
                    return "Failed";
                case ApprenticeshipQAStatus.Passed:
                    return "Passed";
                case ApprenticeshipQAStatus.UnableToComplete:
                    return "Unable to complete";
                default:
                    throw new NotImplementedException($"Unknown value: '{status}'.");
            }
        }
    }
}
