using System;
using System.Collections.Generic;
using OneOf;

namespace Dfc.CourseDirectory.Core.Models
{
    public class ApprenticeshipQASubmission
    {
        public int ApprenticeshipQASubmissionId { get; set; }
        public Guid ProviderId { get; set; }
        public DateTime SubmittedOn { get; set; }
        public UserInfo SubmittedByUser { get; set; }
        public string ProviderMarketingInformation { get; set; }
        public bool? Passed { get; set; }
        public UserInfo LastAssessedBy { get; set; }
        public DateTime? LastAssessedOn { get; set; }
        public bool? ProviderAssessmentPassed { get; set; }
        public bool? ApprenticeshipAssessmentsPassed { get; set; }
        public IReadOnlyCollection<ApprenticeshipQASubmissionApprenticeship> Apprenticeships { get; set; }
        public bool HidePassedNotification { get; set; }
    }

    public class ApprenticeshipQASubmissionApprenticeship
    {
        public int ApprenticeshipQASubmissionApprenticeshipId { get; set; }
        public Guid ApprenticeshipId { get; set; }
        public string ApprenticeshipTitle { get; set; }
        public string ApprenticeshipMarketingInformation { get; set; }
        public IReadOnlyCollection<ApprenticeshipQASubmissionApprenticeshipLocation> Locations { get; set; }
    }

    public class ApprenticeshipQASubmissionApprenticeshipLocation :
        OneOfBase<ApprenticeshipQASubmissionApprenticeshipClassroomLocation, ApprenticeshipQASubmissionApprenticeshipEmployerLocation>
    {
        public ApprenticeshipQASubmissionApprenticeshipLocation(
            ApprenticeshipQASubmissionApprenticeshipClassroomLocation classroomLocation)
            : base(0, value0: classroomLocation)
        {
        }

        public ApprenticeshipQASubmissionApprenticeshipLocation(
            ApprenticeshipQASubmissionApprenticeshipEmployerLocation employerLocation)
            : base(1, value1: employerLocation)
        {
        }

        public bool IsClassroomBased => IsT0;

        public bool IsEmployerBased => IsT1;
    }

    public class ApprenticeshipQASubmissionApprenticeshipClassroomLocation
    {
        public string VenueName { get; set; }
        public int Radius { get; set; }
        public IReadOnlyCollection<ApprenticeshipDeliveryMode> DeliveryModes { get; set; }
    }

    public class ApprenticeshipQASubmissionApprenticeshipEmployerLocation :
        OneOfBase<National, ApprenticeshipQASubmissionApprenticeshipEmployerLocationRegions>
    {
        public ApprenticeshipQASubmissionApprenticeshipEmployerLocation(National national)
            : base(0, value0: national)
        {
        }

        public ApprenticeshipQASubmissionApprenticeshipEmployerLocation(
            ApprenticeshipQASubmissionApprenticeshipEmployerLocationRegions regions)
            : base(1, value1: regions)
        {
        }

        public bool IsNational => IsT0;

        public bool HasRegions => IsT1;
    }

    public class ApprenticeshipQASubmissionApprenticeshipEmployerLocationRegions
    {
        public IReadOnlyCollection<string> SubRegionIds { get; set; }
    }
}
