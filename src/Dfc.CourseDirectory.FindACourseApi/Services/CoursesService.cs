using System;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.FindACourseApi.Helpers;
using Dfc.CourseDirectory.FindACourseApi.Interfaces;
using Dfc.CourseDirectory.FindACourseApi.Models;
using Dfc.CourseDirectory.FindACourseApi.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Dfc.CourseDirectory.FindACourseApi.Services
{
    public class CoursesService : ICourseService
    {
        private readonly ICourseServiceSettings _courseServiceSettings;
        private readonly SearchServiceWrapper _searchServiceWrapper;

        public CoursesService(
            SearchServiceWrapper searchServiceWrapper,
            IOptions<CourseServiceSettings> courseServiceSettings)
        {
            _courseServiceSettings = courseServiceSettings.Value;
            _searchServiceWrapper = searchServiceWrapper;
        }

        public async Task<AzureSearchCourseDetail> CourseDetail(Guid courseId, Guid courseRunId)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _courseServiceSettings.ApiKey);
                var response = await httpClient.GetAsync($"{_courseServiceSettings.ApiUrl}CourseDetail?CourseId={courseId}&RunId={courseRunId}");

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }

                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<AzureSearchCourseDetail>(json);
            }
        }

        public Task<FACSearchResult> CourseSearch(ILogger log, SearchCriteriaStructure criteria)
        {
            return _searchServiceWrapper.SearchCourses(criteria);
        }
    }
}
