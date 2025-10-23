using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.Testing
{
    public partial class TestData
    {
        public Task CreatePostcodeInfo(
            string postcode,
            double latitude,
            double longitude,
            bool inEngland = true)
        {
            return WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(new UpsertPostcodes()
            {
                Records = new[]
                {
                    new UpsertPostcodesRecord()
                    {
                        Postcode = postcode,
                        Position = (latitude, longitude),
                        InEngland = inEngland
                    }
                }
            }));
        }
    }
}
