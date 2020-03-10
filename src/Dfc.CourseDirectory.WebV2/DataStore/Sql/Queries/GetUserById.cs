using Dfc.CourseDirectory.WebV2.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries
{
    public class GetUserById : ISqlQuery<OneOf<NotFound, UserInfo>>
    {
        public string UserId { get; set; }
    }
}