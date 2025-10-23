using System;
using Dfc.CourseDirectory.Core.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class SetProviderDisplayNameSource : ISqlQuery<OneOf<NotFound, Success>>
    {
        public Guid ProviderId { get; set; }
        public ProviderDisplayNameSource DisplayNameSource { get; set; }
    }
}
