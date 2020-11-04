using System;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Services.Models.Courses;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Dfc.CourseDirectory.Services.CourseTextService
{
    public class CourseTextService : ICourseTextService
    {
        private readonly ILogger<CourseTextService> _logger;
        private readonly CourseTextServiceSettings _settings;
        private readonly HttpClient _httpClient;
        private readonly Uri _getYourCourseTextUri;

        public CourseTextService(
            ILogger<CourseTextService> logger,
            HttpClient httpClient,
            IOptions<CourseTextServiceSettings> settings)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (httpClient == null)
            {
                throw new ArgumentNullException(nameof(httpClient));
            }

            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            _logger = logger;
            _settings = settings.Value;
            _httpClient = httpClient;
            _getYourCourseTextUri = settings.Value.GetCourseTextUri();
        }

        public async Task<Result<CourseText>> GetCourseTextByLARS(CourseTextSearchCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException(nameof(criteria));
            }

            if (string.IsNullOrWhiteSpace(criteria.LARSRef))
            {
                throw new ArgumentNullException($"{nameof(criteria.LARSRef)} cannot be null or empty or whitespace.", nameof(criteria.LARSRef));
            }

            try
            {
                if (criteria.LARSRef == "")
                    return Result.Fail<CourseText>("Blank LARS Ref");

                _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _settings.ApiKey);
                var response = await _httpClient.GetAsync(new Uri(_getYourCourseTextUri.AbsoluteUri + "?LARS=" + criteria.LARSRef));

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    var courseText = JsonConvert.DeserializeObject<CourseText>(json);

                    return Result.Ok(courseText);
                }
                else
                {
                    return Result.Fail<CourseText>("Get your courses service unsuccessful http response");
                }
            }
            catch (HttpRequestException hre)
            {
                _logger.LogError(hre, "Get your courses service http request error");
                return Result.Fail<CourseText>("Get your courses service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Get your courses service unknown error.");
                return Result.Fail<CourseText>("Get your courses service unknown error.");
            }
        }
    }
}
