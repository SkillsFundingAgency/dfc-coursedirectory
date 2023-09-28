using System;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class UpdateProviderType : ISqlQuery<OneOf<NotFound, Success>>
    {
        public Guid ProviderId { get; set; }
        public ProviderType ProviderType { get; set; }
        public UserInfo UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
    }
}
