using Dfc.CourseDirectory.Common.Interfaces;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Services.Interfaces
{
    public interface IPostCodeSearchService
    {
        Task<IResult<IPostCodeSearchResult>> SearchAsync(IPostCodeSearchCriteria criteria);
        Task<IResult<IAddressSelectionResult>> RetrieveAsync(IAddressSelectionCriteria criteria);
    }
}