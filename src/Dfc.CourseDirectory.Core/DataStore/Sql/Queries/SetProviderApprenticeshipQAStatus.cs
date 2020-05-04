using System;
using Dfc.CourseDirectory.Core.Models;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class SetProviderApprenticeshipQAStatus : ISqlQuery<None>
    {
        public Guid ProviderId { get; set; }
        public ApprenticeshipQAStatus ApprenticeshipQAStatus { get; set; }
    }
}
