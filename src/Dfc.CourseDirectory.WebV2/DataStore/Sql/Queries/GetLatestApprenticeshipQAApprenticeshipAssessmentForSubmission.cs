using System;
using Dfc.CourseDirectory.WebV2.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries
{
    public class GetLatestApprenticeshipQAApprenticeshipAssessmentForSubmission
        : ISqlQuery<OneOf<None, ApprenticeshipQAApprenticeshipAssessment>>
    {
        public Guid ApprenticeshipId { get; set; }
        public int ApprenticeshipQASubmissionId { get; set; }
    }
}
