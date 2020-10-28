
using Dfc.ProviderPortal.FindACourse.Helpers;
using Dfc.ProviderPortal.FindACourse.Interfaces;
using Dfc.ProviderPortal.FindACourse.Models;
using Dfc.ProviderPortal.FindACourse.Settings;
using Dfc.ProviderPortal.Packages;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Search.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Document = Microsoft.Azure.Documents.Document;


namespace Dfc.ProviderPortal.FindACourse.Services
{
    public class CoursesService : ICourseService
    {
        //private readonly ILogger _log;
        private readonly ICosmosDbHelper _cosmosDbHelper;
        private readonly ICosmosDbCollectionSettings _settings;
        private readonly IProviderServiceSettings _providerServiceSettings;
        private readonly IVenueServiceSettings _venueServiceSettings;
        private readonly ISearchServiceSettings _searchServiceSettings;
        private readonly IQualificationServiceSettings _qualServiceSettings;
        private readonly ICourseServiceSettings _courseServiceSettings;
        private readonly SearchServiceWrapper _searchServiceWrapper;

        public CoursesService(
            //ILogger log,
            ICosmosDbHelper cosmosDbHelper,
            SearchServiceWrapper searchServiceWrapper,
            IOptions<ProviderServiceSettings> providerServiceSettings,
            IOptions<VenueServiceSettings> venueServiceSettings,
            IOptions<SearchServiceSettings> searchServiceSettings,
            IOptions<QualificationServiceSettings> qualServiceSettings,
            IOptions<CosmosDbCollectionSettings> settings,
            IOptions<CourseServiceSettings> courseServiceSettings)
        {
            //Throw.IfNull(log, nameof(log));
            Throw.IfNull(cosmosDbHelper, nameof(cosmosDbHelper));
            Throw.IfNull(searchServiceWrapper, nameof(searchServiceWrapper));
            Throw.IfNull(settings, nameof(settings));
            Throw.IfNull(providerServiceSettings, nameof(providerServiceSettings));
            Throw.IfNull(venueServiceSettings, nameof(venueServiceSettings));
            Throw.IfNull(qualServiceSettings, nameof(qualServiceSettings));
            Throw.IfNull(searchServiceSettings, nameof(searchServiceSettings));

            //_log = log;
            _cosmosDbHelper = cosmosDbHelper;
            _settings = settings.Value;
            _providerServiceSettings = providerServiceSettings.Value;
            _venueServiceSettings = venueServiceSettings.Value;
            _qualServiceSettings = qualServiceSettings.Value;
            _searchServiceSettings = searchServiceSettings.Value;
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

        public async Task<Provider> ProviderDetail(string PRN)
        {
            // Call service to get data
            StringContent content = new StringContent(JsonConvert.SerializeObject(new { PRN }),
                                                      Encoding.UTF8,
                                                      "application/json");
            Task<HttpResponseMessage> taskResponse = new HttpClient().GetAsync($"{_providerServiceSettings.ApiUrl}GetProviderByPRN?code={_courseServiceSettings.ApiKey}&PRN={PRN}");
                                                                                //content);
            taskResponse.Wait();
            Task<string> taskJSON = taskResponse.Result.Content.ReadAsStringAsync();
            taskJSON.Wait();
            return JsonConvert.DeserializeObject<Provider>(taskJSON.Result);
        }

        private IEnumerable<AzureSearchVenueModel> GetVenues(ILogger log, IEnumerable<CourseRun> runs = null)
        {
            IVenueServiceWrapper service = new VenueServiceWrapper(_venueServiceSettings);
            IEnumerable<CourseRun> venueruns = runs?.Where(x => x.VenueId != null);
            log.LogInformation($"Getting data for { venueruns?.Count().ToString() ?? "all" } venues");

            // Get all venues to save time & RUs if there's too many to get by Id
            if (venueruns == null || venueruns.Count() > _searchServiceSettings.ThresholdVenueCount)
                return service.GetVenues();
            else {
                List<AzureSearchVenueModel> venues = new List<AzureSearchVenueModel>();
                foreach (CourseRun r in venueruns)
                    if (!venues.Any(x => x.id == r.VenueId.Value)) {
                        AzureSearchVenueModel venue = service.GetById<AzureSearchVenueModel>(r.VenueId.Value);
                        if (venue != null)
                            venues.Add(venue);
                    }
                log.LogInformation($"Successfully retrieved data for {venues.Count()} venues");
                return venues;
            }
        }

        public Task<FACSearchResult> CourseSearch(ILogger log, SearchCriteriaStructure criteria)
        {
            return _searchServiceWrapper.SearchCourses(criteria);
        }

        public Task<ProviderSearchResult> ProviderSearch(ILogger log, ProviderSearchCriteriaStructure criteria)
        {
            return _searchServiceWrapper.SearchProviders(criteria);
        }

        public Task<LARSSearchResult> LARSSearch(ILogger log, LARSSearchCriteriaStructure criteria)
        {
            return _searchServiceWrapper.SearchLARS(criteria);
        }

        public Task<PostcodeSearchResult> PostcodeSearch(ILogger log, PostcodeSearchCriteriaStructure criteria)
        {
            return _searchServiceWrapper.SearchPostcode(criteria);
        }

        public async Task<IEnumerable<ICourse>> GetAllCourses(ILogger log)
        {
            try {
                // Get all course documents in the collection
                string token = null;
                Task<FeedResponse<dynamic>> task = null;
                List<dynamic> docs = new List<dynamic>();
                log.LogInformation("Getting all courses from collection");

                // Read documents in batches, using continuation token to make sure we get them all
                using (DocumentClient client = _cosmosDbHelper.GetClient()) {
                    do {
                        task = client.ReadDocumentFeedAsync(UriFactory.CreateDocumentCollectionUri("providerportal", _settings.CoursesCollectionId),
                                                            new FeedOptions { MaxItemCount = -1, RequestContinuation = token });
                        token = task.Result.ResponseContinuation;
                        log.LogInformation("Collating results");
                        docs.AddRange(task.Result.ToList());
                    } while (token != null);
                }

                // Cast the returned data by serializing to json and then deserialising into Course objects
                log.LogInformation($"Serializing data for {docs.LongCount()} courses");
                string json = JsonConvert.SerializeObject(docs);
                return JsonConvert.DeserializeObject<IEnumerable<Course>>(json);

            } catch (Exception ex) {
                throw ex;
            }
        }

        public async Task<ICourse> AddCourse(ICourse course)
        {
            Throw.IfNull(course, nameof(course));

            Course persisted;

            using (var client = _cosmosDbHelper.GetClient())
            {
                await _cosmosDbHelper.CreateDatabaseIfNotExistsAsync(client);
                await _cosmosDbHelper.CreateDocumentCollectionIfNotExistsAsync(client, _settings.CoursesCollectionId);
                var doc = await _cosmosDbHelper.CreateDocumentAsync(client, _settings.CoursesCollectionId, course);
                persisted = _cosmosDbHelper.DocumentTo<Course>(doc);
            }

            return persisted;
        }

        public async Task<ICourse> GetCourseById(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException($"Cannot be an empty {nameof(Guid)}", nameof(id));

            Course persisted = null;

            using (var client = _cosmosDbHelper.GetClient())
            {
                var doc = _cosmosDbHelper.GetDocumentById(client, _settings.CoursesCollectionId, id);
                persisted = _cosmosDbHelper.DocumentTo<Course>(doc);
            }

            return persisted;
        }

        //public async Task<AzureSearchCourseDetail> GetCourseSearchDataById(Guid CourseId, Guid RunId)
        //{
        //    if (CourseId == Guid.Empty)
        //        throw new ArgumentException($"Cannot be an empty {nameof(Guid)}", nameof(CourseId));
        //    if (RunId == Guid.Empty)
        //        throw new ArgumentException($"Cannot be an empty {nameof(Guid)}", nameof(RunId));

        //    Course course = null;
        //    dynamic venue = null;

        //    using (var client = _cosmosDbHelper.GetClient()) {
        //        var doc = _cosmosDbHelper.GetDocumentById(client, _settings.CoursesCollectionId, CourseId);
        //        course = _cosmosDbHelper.DocumentTo<Course>(doc);
        //    }

        //    //CourseRun run = course.CourseRuns.FirstOrDefault(r => r.id == RunId);
        //    Guid? venueid = course.CourseRuns
        //                          .Where(r => r.id == RunId && r.VenueId != null)
        //                          .FirstOrDefault()
        //                          ?.VenueId;
        //    if (venueid.HasValue)
        //        venue = (dynamic)new VenueServiceWrapper(_venueServiceSettings).GetById<dynamic>(venueid.Value);
        //    var provider = new ProviderServiceWrapper(_providerServiceSettings).GetByPRN(course.ProviderUKPRN);
        //    var qualification = new QualificationServiceWrapper(_qualServiceSettings).GetQualificationById(course.LearnAimRef);

        //    //return from Course c in new List<Course>() { course }
        //    //       from CourseRun r in c.CourseRuns
        //    //       from AzureSearchProviderModel p in new List<AzureSearchProviderModel>() { provider }
        //    //       from AzureSearchVenueModel v in venues
        //    //       select new AzureSearchCourseDetail();
        //    return new AzureSearchCourseDetail()
        //    {
        //        Course = course,
        //        Provider = provider,
        //        Qualification = qualification,
        //        Venue = venue
        //    };
        //}

        public async Task<ICourse> Update(ICourse course)
        {
            Throw.IfNull(course, nameof(course));
         
            Course updated = null;

            using (var client = _cosmosDbHelper.GetClient())
            {
                var updatedDocument = await _cosmosDbHelper.UpdateDocumentAsync(client, _settings.CoursesCollectionId, course);

                updated = _cosmosDbHelper.DocumentTo<Course>(updatedDocument);
            }

            return updated;

        }

        public async Task<IEnumerable<ICourse>> GetCoursesByUKPRN(int UKPRN)
        {
            Throw.IfNull<int>(UKPRN, nameof(UKPRN));
            Throw.IfLessThan(0, UKPRN, nameof(UKPRN));

            IEnumerable<Course> persisted = null;
            using (var client = _cosmosDbHelper.GetClient())
            {
                var docs = _cosmosDbHelper.GetDocumentsByUKPRN(client, _settings.CoursesCollectionId, UKPRN);
                persisted = docs;
            }

            return persisted;
        }

        public async Task<List<string>> DeleteCoursesByUKPRN(int UKPRN)
        {
            Throw.IfNull<int>(UKPRN, nameof(UKPRN));
            Throw.IfLessThan(0, UKPRN, nameof(UKPRN));

            List<string> results = null;
            using (var client = _cosmosDbHelper.GetClient())
            {
               results = await _cosmosDbHelper.DeleteDocumentsByUKPRN(client, _settings.CoursesCollectionId, UKPRN);
            }

            return results;
        }
    }
}
