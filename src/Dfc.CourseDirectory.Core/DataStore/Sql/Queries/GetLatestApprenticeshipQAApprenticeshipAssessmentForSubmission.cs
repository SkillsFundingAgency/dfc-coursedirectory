using System;
using Dfc.CourseDirectory.Core.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetLatestApprenticeshipQAApprenticeshipAssessmentForSubmission
        : ISqlQuery<OneOf<None, ApprenticeshipQAApprenticeshipAssessment>>
    {
        public Guid ApprenticeshipId { get; set; }
        public int ApprenticeshipQASubmissionId { get; set; }
    }
}
