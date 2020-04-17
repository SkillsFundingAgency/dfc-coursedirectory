using Dfc.CourseDirectory.Core.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetUserById : ISqlQuery<OneOf<NotFound, UserInfo>>
    {
        public string UserId { get; set; }
    }
}