using System;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetLatestApprenticeshipQASubmissionForProvider : ISqlQuery<ApprenticeshipQASubmission>
    {
        public Guid ProviderId { get; set; }
    }
}
