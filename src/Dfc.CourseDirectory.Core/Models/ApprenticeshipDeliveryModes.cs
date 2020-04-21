using System;

namespace Dfc.CourseDirectory.Core.Models
{
    [Flags]
    public enum ApprenticeshipDeliveryModes
    {
        None = 0,
        EmployerAddress = 1,
        DayRelease = 2,
        BlockRelease = 4
    }

    public static class ApprenticeshipDeliveryModesExtensions
    {
        public static string ToDisplayName(this ApprenticeshipDeliveryModes apprenticeshipDeliveryModes) =>
            apprenticeshipDeliveryModes switch
            {
                ApprenticeshipDeliveryModes.None => string.Empty,
                ApprenticeshipDeliveryModes.EmployerAddress => "Employer Address",
                ApprenticeshipDeliveryModes.DayRelease => "Day Release",
                ApprenticeshipDeliveryModes.BlockRelease => "Block Release",
                _ => throw new NotSupportedException($"Unknown value: '{apprenticeshipDeliveryModes}'.")
            };
    }
}
