using Dfc.CourseDirectory.Common.Interfaces;

namespace Dfc.CourseDirectory.Services.Interfaces.OnspdService
{
    public interface IOnspdService
    {
        IResult<IOnspdSearchResult> GetOnspdData(IOnspdSearchCriteria criteria);
    }
}
