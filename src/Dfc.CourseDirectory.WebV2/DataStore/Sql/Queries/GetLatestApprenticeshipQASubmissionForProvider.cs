using System;
using Dfc.CourseDirectory.WebV2.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries
{
    public class GetLatestApprenticeshipQASubmissionForProvider : ISqlQuery<OneOf<None, ApprenticeshipQASubmission>>
    {
        public Guid ProviderId { get; set; }
    }
}
