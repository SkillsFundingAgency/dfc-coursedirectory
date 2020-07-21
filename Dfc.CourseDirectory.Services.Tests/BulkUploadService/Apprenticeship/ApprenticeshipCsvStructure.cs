using System.Collections.Generic;

namespace Dfc.CourseDirectory.Services.Tests.BulkUploadService.Apprenticeship
{
    internal static class ApprenticeshipCsvStructure
    {
        public static readonly IReadOnlyList<string> Fields = new List<string>
        {
            "STANDARD_CODE",
            "STANDARD_VERSION",
            "FRAMEWORK_CODE",
            "FRAMEWORK_PROG_TYPE",
            "FRAMEWORK_PATHWAY_CODE",
            "APPRENTICESHIP_INFORMATION",
            "APPRENTICESHIP_WEBPAGE",
            "CONTACT_EMAIL",
            "CONTACT_PHONE",
            "CONTACT_URL",
            "DELIVERY_METHOD",
            "VENUE",
            "RADIUS",
            "DELIVERY_MODE",
            "ACROSS_ENGLAND",
            "NATIONAL_DELIVERY",
            "REGION",
            "SUB_REGION",
        };
    }
}
