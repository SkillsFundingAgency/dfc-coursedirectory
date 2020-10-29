using System.Threading.Tasks;
using Dfc.CourseDirectory.FindACourseApi.Models;
using Dfc.CourseDirectory.FindACourseApi.Models.Search.Faoc;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.FindACourseApi.Interfaces.Faoc
{
    public interface IOnlineCourseService
    {
        Task<FaocSearchResult> OnlineCourseSearch(ILogger log, OnlineCourseSearchCriteria criteria); // string SearchText);
    }
}