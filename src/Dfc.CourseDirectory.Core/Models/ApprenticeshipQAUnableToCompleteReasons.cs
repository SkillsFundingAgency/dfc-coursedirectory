using System;

namespace Dfc.CourseDirectory.Core.Models
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

    public static class ApprenticeshipQAUnableToCompleteReasonsExtensions
    {
        public static string ToDisplayName(this ApprenticeshipQAUnableToCompleteReasons status)
        {
            switch (status)
            {
                case ApprenticeshipQAUnableToCompleteReasons.None:
                    return "None";
                case ApprenticeshipQAUnableToCompleteReasons.Other:
                    return "Other";
                case ApprenticeshipQAUnableToCompleteReasons.ProviderDevelopingProvision:
                    return "Provider Developing Provision";
                case ApprenticeshipQAUnableToCompleteReasons.ProviderHasAppliedToTheWrongRoute:
                    return "Provider has applied to the wrong route";
                case ApprenticeshipQAUnableToCompleteReasons.ProviderHasWithdrawnApplication:
                    return "Provider has withdrawn application";
                case ApprenticeshipQAUnableToCompleteReasons.StandardNotReady:
                    return "Standard not ready";
                default:
                    throw new NotImplementedException($"Unknown value: '{status}'.");
            }
        }
    }
}
