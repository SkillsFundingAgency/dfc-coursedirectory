using System.Threading.Tasks;
using Dfc.ProviderPortal.FindACourse.Models;
using Dfc.ProviderPortal.FindACourse.Models.Search.Faoc;
using Microsoft.Extensions.Logging;

namespace Dfc.ProviderPortal.FindACourse.Interfaces.Faoc
{
    public interface IOnlineCourseService
    {
        Task<FaocSearchResult> OnlineCourseSearch(ILogger log, OnlineCourseSearchCriteria criteria); // string SearchText);
    }
}