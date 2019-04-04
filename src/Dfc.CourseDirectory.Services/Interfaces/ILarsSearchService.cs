using Dfc.CourseDirectory.Common.Interfaces;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Services.Interfaces
{
    public interface ILarsSearchService
    {
        Task<IResult<ILarsSearchResult>> SearchAsync(ILarsSearchCriteria criteria);

        Task<IResult<ILarsSearchResult>> SearchAsync(IZCodeSearchCriteria criteria);
    }
}