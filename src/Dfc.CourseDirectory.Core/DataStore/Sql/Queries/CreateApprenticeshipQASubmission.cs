using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.Models;
using OneOf;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class CreateApprenticeshipQASubmission : ISqlQuery<int>
    {
        public Guid ProviderId { get; set; }
        public DateTime SubmittedOn { get; set; }
        public string SubmittedByUserId { get; set; }
        public string ProviderMarketingInformation { get; set; }
        public IEnumerable<CreateApprenticeshipQASubmissionApprenticeship> Apprenticeships { get; set; }
    }

    public class CreateApprenticeshipQASubmissionApprenticeship
    {
        public Guid ApprenticeshipId { get; set; }
        public string ApprenticeshipTitle { get; set; }
        public string ApprenticeshipMarketingInformation { get; set; }
        public IReadOnlyCollection<CreateApprenticeshipQASubmissionApprenticeshipLocation> Locations { get; set; }
    }

    public class CreateApprenticeshipQASubmissionApprenticeshipLocation :
        OneOfBase<CreateApprenticeshipQASubmissionApprenticeshipClassroomLocation, CreateApprenticeshipQASubmissionApprenticeshipEmployerLocation>
    {
        public CreateApprenticeshipQASubmissionApprenticeshipLocation(
            CreateApprenticeshipQASubmissionApprenticeshipClassroomLocation classroomLocation)
            : base(0, value0: classroomLocation)
        {
        }

        public CreateApprenticeshipQASubmissionApprenticeshipLocation(
            CreateApprenticeshipQASubmissionApprenticeshipEmployerLocation employerLocation)
            : base(1, value1: employerLocation)
        {
        }

        public bool IsClassroomBased => IsT0;

        public bool IsEmployerBased => IsT1;
    }

    public class CreateApprenticeshipQASubmissionApprenticeshipClassroomLocation
    {
        public string VenueName { get; set; }
        public int Radius { get; set; }
        public IEnumerable<ApprenticeshipDeliveryMode> DeliveryModes { get; set; }
    }

    public class CreateApprenticeshipQASubmissionApprenticeshipEmployerLocation :
        OneOfBase<National, CreateApprenticeshipQASubmissionApprenticeshipEmployerLocationRegions>
    {
        public CreateApprenticeshipQASubmissionApprenticeshipEmployerLocation(National national)
            : base(0, value0: national)
        {
        }

        public CreateApprenticeshipQASubmissionApprenticeshipEmployerLocation(
            CreateApprenticeshipQASubmissionApprenticeshipEmployerLocationRegions regions)
            : base(1, value1: regions)
        {
        }
    }

    public class CreateApprenticeshipQASubmissionApprenticeshipEmployerLocationRegions
    {
        public IReadOnlyCollection<string> SubRegionIds { get; set; }
    }
}
