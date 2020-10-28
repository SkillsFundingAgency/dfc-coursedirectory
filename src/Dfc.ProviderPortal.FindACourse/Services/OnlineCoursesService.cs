using Dfc.ProviderPortal.FindACourse.Interfaces;
using Dfc.ProviderPortal.FindACourse.Settings;
using Dfc.ProviderPortal.Packages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Dfc.ProviderPortal.FindACourse.Helpers.Faoc;
using Dfc.ProviderPortal.FindACourse.Interfaces.Faoc;
using Dfc.ProviderPortal.FindACourse.Models.Search.Faoc;


namespace Dfc.ProviderPortal.FindACourse.Services
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
