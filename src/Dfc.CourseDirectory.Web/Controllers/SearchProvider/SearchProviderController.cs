using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Search;
using Dfc.CourseDirectory.Web.ViewComponents.SearchProviderResults;
using Dfc.CourseDirectory.Web.ViewModels.SearchProvider;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.Web.Controllers.SearchProvider
{
    public class SearchProviderController : Controller
    {
        private readonly ISearchClient<ProviderSearchQuery, Core.Search.Models.Provider> _searchClient;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ILogger<SearchProviderController> _logger;

        private ISession _session => _contextAccessor.HttpContext.Session;

        public SearchProviderController(
            ISearchClient<ProviderSearchQuery, Core.Search.Models.Provider> searchClient,
            IHttpContextAccessor contextAccessor,
            ILogger<SearchProviderController> logger)
        {
            _searchClient = searchClient ?? throw new System.ArgumentNullException(nameof(searchClient));
            _contextAccessor = contextAccessor ?? throw new System.ArgumentNullException(nameof(contextAccessor));
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }

        [Authorize]
        public async Task<IActionResult> Index([FromQuery] string keyword)
        {
            if (_session.GetInt32("UKPRN").HasValue)
            {
                _session.Remove("UKPRN");
            }

            if (string.IsNullOrWhiteSpace(keyword))
            {
                return View(SearchProviderResultsViewModel.Empty());
            }

            var result = await _searchClient.Search(new ProviderSearchQuery
            {
                SearchText = keyword
            });

            return View(new SearchProviderResultsViewModel
            {
                Search = keyword,
                Providers = result.Results.Select(SearchProviderResultViewModel.FromProvider)
            });
        }

        [Authorize]
        public async Task<IActionResult> Search([FromQuery] string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return ViewComponent(nameof(SearchProviderResults), SearchProviderResultsViewModel.Empty());
            }

            var result = await _searchClient.Search(new ProviderSearchQuery
            {
                SearchText = keyword
            });

            return ViewComponent(nameof(SearchProviderResults), new SearchProviderResultsViewModel
            {
                Search = keyword,
                Providers = result.Results.Select(SearchProviderResultViewModel.FromProvider)
            });
        }
    }
}