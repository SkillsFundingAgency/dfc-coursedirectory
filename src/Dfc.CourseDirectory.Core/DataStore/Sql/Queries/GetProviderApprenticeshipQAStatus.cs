using System;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetProviderApprenticeshipQAStatus : ISqlQuery<ApprenticeshipQAStatus?>
    {
        public Guid ProviderId { get; set; }
    }
}
