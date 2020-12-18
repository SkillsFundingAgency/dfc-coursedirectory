using System;
using Dfc.CourseDirectory.Core.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class DeleteTLevel : ISqlQuery<OneOf<NotFound, Success>>
    {
        public Guid TLevelId { get; set; }
        public UserInfo DeletedBy { get; set; }
        public DateTime DeletedOn { get; set; }
    }
}
