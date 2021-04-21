using System;

namespace Dfc.CourseDirectory.Core.Models
{
    [Flags]
    public enum ApprenticeshipLocationType
    {
        ClassroomBased = 1,
        EmployerBased = 2,
        ClassroomBasedAndEmployerBased = ClassroomBased | EmployerBased
    }

    public static class ApprenticeshipLocationTypeExtensions
    {
        public static string ToDescription(this ApprenticeshipLocationType apprenticeshipLocationType) =>
            apprenticeshipLocationType switch
            {
                ApprenticeshipLocationType.EmployerBased => "Employer locations",
                ApprenticeshipLocationType.ClassroomBased => "Your venues",
                ApprenticeshipLocationType.ClassroomBasedAndEmployerBased => "Your venues and employer locations",
                _ => throw new NotImplementedException($"Unknown value: '{apprenticeshipLocationType}'.")
            };
    }
}
