using Dfc.CourseDirectory.Core.DataStore.Sql.Models;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.Queries
{
    public class GetPostcodeInfo : ISqlQuery<PostcodeInfo>
    {
        public string Postcode { get; set; }
    }
}
