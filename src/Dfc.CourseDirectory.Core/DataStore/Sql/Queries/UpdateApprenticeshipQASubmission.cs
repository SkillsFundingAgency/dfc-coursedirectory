using System;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class UpdateApprenticeshipQASubmission : ISqlQuery<OneOf<NotFound, Success>>
    {
        public int ApprenticeshipQASubmissionId { get; set; }
        public bool? Passed { get; set; }
        public string LastAssessedByUserId { get; set; }
        public DateTime? LastAssessedOn { get; set; }
        public bool? ProviderAssessmentPassed { get; set; }
        public bool? ApprenticeshipAssessmentsPassed { get; set; }
    }
}
