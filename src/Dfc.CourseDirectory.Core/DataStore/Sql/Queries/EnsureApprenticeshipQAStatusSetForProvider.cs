using System;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class EnsureApprenticeshipQAStatusSetForProvider : ISqlQuery<None>
    {
        public Guid ProviderId { get; set; }
    }
}
