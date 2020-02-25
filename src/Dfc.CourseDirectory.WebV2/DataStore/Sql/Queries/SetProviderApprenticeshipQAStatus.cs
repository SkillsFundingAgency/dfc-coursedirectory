using System;
using Dfc.CourseDirectory.WebV2.Models;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries
{
    public class SetProviderApprenticeshipQAStatus : ISqlQuery<None>
    {
        public Guid ProviderId { get; set; }
        public ApprenticeshipQAStatus ApprenticeshipQAStatus { get; set; }
    }
}
