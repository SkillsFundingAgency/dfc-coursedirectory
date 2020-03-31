using System;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries
{
    public class EnsureProviderExists : ISqlQuery<None>
    {
        public Guid ProviderId { get; set; }
    }
}
