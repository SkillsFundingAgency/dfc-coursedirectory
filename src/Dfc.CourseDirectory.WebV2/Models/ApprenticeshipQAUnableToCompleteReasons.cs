using System;

namespace Dfc.CourseDirectory.WebV2.Models
{
    [Flags]
    public enum ApprenticeshipQAUnableToCompleteReasons
    {
        None = 0,
        StandardNotReady = 1,
        ProviderDevelopingProvision = 2,
        ProviderHasWithdrawnApplication = 4,
        ProviderHasAppliedToTheWrongRoute = 8,
        Other = 16
    }
}
