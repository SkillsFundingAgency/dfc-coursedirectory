using Dfc.CourseDirectory.FindACourseApi.Interfaces;
using Dfc.CourseDirectory.FindACourseApi.Settings;
using Dfc.ProviderPortal.Packages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Dfc.CourseDirectory.FindACourseApi.Helpers.Faoc;
using Dfc.CourseDirectory.FindACourseApi.Interfaces.Faoc;
using Dfc.CourseDirectory.FindACourseApi.Models.Search.Faoc;


namespace Dfc.CourseDirectory.FindACourseApi.Services
{
    public class OnlineCoursesService : IOnlineCourseService
    {
        //private readonly ILogger _log;
        private readonly OnlineCourseSearchServiceWrapper _courseSearchServiceWrapper;

        public OnlineCoursesService(
            ICosmosDbHelper cosmosDbHelper,
            OnlineCourseSearchServiceWrapper courseSearchServiceWrapper,
            IOptions<ProviderServiceSettings> providerServiceSettings,
            IOptions<VenueServiceSettings> venueServiceSettings,
            IOptions<SearchServiceSettings> searchServiceSettings,
            IOptions<QualificationServiceSettings> qualServiceSettings,
            IOptions<CosmosDbCollectionSettings> settings,
            IOptions<CourseServiceSettings> courseServiceSettings)
        {
            Throw.IfNull(cosmosDbHelper, nameof(cosmosDbHelper));
            Throw.IfNull(courseSearchServiceWrapper, nameof(courseSearchServiceWrapper));
            Throw.IfNull(settings, nameof(settings));
            Throw.IfNull(providerServiceSettings, nameof(providerServiceSettings));
            Throw.IfNull(venueServiceSettings, nameof(venueServiceSettings));
            Throw.IfNull(qualServiceSettings, nameof(qualServiceSettings));
            Throw.IfNull(searchServiceSettings, nameof(searchServiceSettings));

            _courseSearchServiceWrapper = courseSearchServiceWrapper;
        }
        
        public Task<FaocSearchResult> OnlineCourseSearch(ILogger log, OnlineCourseSearchCriteria criteria)
        {
            return _courseSearchServiceWrapper.SearchOnlineCourses(criteria);
        }
    }
}
