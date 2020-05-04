using System;
using Dfc.CourseDirectory.Core.Models;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class CreateApprenticeshipQAApprenticeshipAssessment : ISqlQuery<Success>
    {
        public int ApprenticeshipQASubmissionId { get; set; }
        public Guid ApprenticeshipId { get; set; }
        public DateTime AssessedOn { get; set; }
        public string AssessedByUserId { get; set; }
        public bool Passed { get; set; }
        public bool CompliancePassed { get; set; }
        public ApprenticeshipQAApprenticeshipComplianceFailedReasons ComplianceFailedReasons { get; set; }
        public string ComplianceComments { get; set; }
        public bool StylePassed { get; set; }
        public ApprenticeshipQAApprenticeshipStyleFailedReasons StyleFailedReasons { get; set; }
        public string StyleComments { get; set; }
    }
}
