
using Dfc.CourseDirectory.FindACourseApi.Models;
using Microsoft.Azure.Search.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Document = Microsoft.Azure.Documents.Document;


namespace Dfc.CourseDirectory.FindACourseApi.Interfaces
{
    public interface ICourseService
    {
        Task<ICourse> AddCourse(ICourse course);
        Task<ICourse> GetCourseById(Guid id);
        //Task<AzureSearchCourseDetail> GetCourseSearchDataById(Guid CourseId, Guid RunId);
        Task<IEnumerable<ICourse>> GetCoursesByUKPRN(int UKPRN);
        Task<List<string>> DeleteCoursesByUKPRN(int UKPRN);
        Task<ICourse> Update(ICourse doc);
        Task<IEnumerable<ICourse>> GetAllCourses(ILogger log);
        //Task<IEnumerable<IAzureSearchCourse>> FindACourseAzureSearchData(ILogger log);
        //Task<IEnumerable<IndexingResult>> UploadCoursesToSearch(ILogger log, IReadOnlyList<Document> documents);
        Task<FACSearchResult> CourseSearch(ILogger log, SearchCriteriaStructure criteria); // string SearchText);
        Task<ProviderSearchResult> ProviderSearch(ILogger log, ProviderSearchCriteriaStructure criteria);
        Task<LARSSearchResult> LARSSearch(ILogger log, LARSSearchCriteriaStructure criteria);
        Task<PostcodeSearchResult> PostcodeSearch(ILogger log, PostcodeSearchCriteriaStructure criteria);
        Task<AzureSearchCourseDetail> CourseDetail(Guid courseId, Guid courseRunId);
        Task<Provider> ProviderDetail(string PRN);
    }
}
