using Dfc.CourseDirectory.Common.Interfaces;
using Dfc.CourseDirectory.Models.Interfaces.Onspd;
using Dfc.CourseDirectory.Models.Models.Onspd;
using Microsoft.Azure.Search;

namespace Dfc.CourseDirectory.Services.Interfaces.OnspdService
{
    public interface IOnspdService
    {
        IResult<IOnspdSearchResult> GetOnspdData(IOnspdSearchCriteria criteria);
        Onspd RunQuery(ISearchIndexClient indexClient, string postcode);
    }
}
