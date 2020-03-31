using System;
using Dfc.CourseDirectory.WebV2.Models;

namespace Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries
{
    public class GetProviderApprenticeshipQAStatus : ISqlQuery<ApprenticeshipQAStatus?>
    {
        public Guid ProviderId { get; set; }
    }
}
