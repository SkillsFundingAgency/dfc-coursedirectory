using System;

namespace Dfc.CourseDirectory.WebV2.Models
{
    [Flags]
    public enum ApprenticeshipQAApprenticeshipComplianceFailedReasons
    {
        None = 0,
        UsedCapitalAForApprenticeships = 1,
        SpecificEmployerNamed = 2,
        UnverifiableClaim = 4,
        IncorrectOfsetGradeUsed = 8,
        InsufficientDetail = 16,
        NotAimedAtEmployer = 32,
        Other = 64
    }
}
