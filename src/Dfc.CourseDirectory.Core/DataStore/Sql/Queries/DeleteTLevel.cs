using System;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class DeleteTLevel : ISqlQuery<OneOf<NotFound, Success>>
    {
        public Guid TLevelId { get; set; }
    }
}
