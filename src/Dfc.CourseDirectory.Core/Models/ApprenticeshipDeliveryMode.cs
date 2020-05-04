using System;

namespace Dfc.CourseDirectory.Core.Models
{
    public enum ApprenticeshipDeliveryMode
    {
        EmployerAddress = 1,
        DayRelease = 2,
        BlockRelease = 3
    }

    public static class ApprenticeshipDeliveryModesExtensions
    {
        public static string ToDisplayName(this ApprenticeshipDeliveryMode apprenticeshipDeliveryMode) =>
            apprenticeshipDeliveryMode switch
            {
                ApprenticeshipDeliveryMode.EmployerAddress => "Employer Address",
                ApprenticeshipDeliveryMode.DayRelease => "Day Release",
                ApprenticeshipDeliveryMode.BlockRelease => "Block Release",
                _ => throw new NotSupportedException($"Unknown value: '{apprenticeshipDeliveryMode}'.")
            };
    }
}
