using System;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetLatestApprenticeshipQAUnableToCompleteInfoForProvider :
        ISqlQuery<ApprenticeshipQAUnableToCompleteInfo>
    {
        public Guid ProviderId { get; set; }
    }
}
