using System;

namespace Dfc.CourseDirectory.WebV2.Models
{
    public enum ApprenticeshipLocationType
    {
        ClassroomBased = 1,
        EmployerBased = 2,
        ClassroomBasedAndEmployerBased = 3
    }

    public static class ApprenticeshipLocationTypeExtensions
    {
        public static string ToDisplayName(this ApprenticeshipLocationType apprenticeshipLocationType) =>
            apprenticeshipLocationType switch
            {
                ApprenticeshipLocationType.EmployerBased => "Employer based locations",
                ApprenticeshipLocationType.ClassroomBased => "Classroom based locations",
                ApprenticeshipLocationType.ClassroomBasedAndEmployerBased => "Employer and classroom based locations",
                _ => throw new NotImplementedException($"Unknown value: '{apprenticeshipLocationType}'.")
            };
    }
}
