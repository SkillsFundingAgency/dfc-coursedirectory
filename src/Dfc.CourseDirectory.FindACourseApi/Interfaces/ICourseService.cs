using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.FindACourseApi.Models;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.FindACourseApi.Interfaces
{
    public interface ICourseService
    {
        Task<FACSearchResult> CourseSearch(ILogger log, SearchCriteriaStructure criteria);
        Task<AzureSearchCourseDetail> CourseDetail(Guid courseId, Guid courseRunId);
    }
}
