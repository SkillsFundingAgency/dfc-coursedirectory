using System;

namespace Dfc.CourseDirectory.Core.Models
{
    [Flags]
    public enum ApprenticeshipQAProviderComplianceFailedReasons
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
