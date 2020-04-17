using System;
using Dfc.CourseDirectory.Core.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetLatestApprenticeshipQASubmissionForProvider : ISqlQuery<OneOf<None, ApprenticeshipQASubmission>>
    {
        public Guid ProviderId { get; set; }
    }
}
