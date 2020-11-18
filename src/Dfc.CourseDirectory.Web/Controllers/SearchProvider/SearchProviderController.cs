using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Search;
using Dfc.CourseDirectory.Core.Search.Models;
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
        private readonly ISearchClient<Provider> _searchClient;
        private readonly ILogger<SearchProviderController> _logger;

        private ISession Session => HttpContext.Session;

        public SearchProviderController(
            ISearchClient<Provider> searchClient,
            ILogger<SearchProviderController> logger)
        {
            _searchClient = searchClient ?? throw new System.ArgumentNullException(nameof(searchClient));
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }

        [Authorize]
        public async Task<IActionResult> Index([FromQuery] string keyword)
        {
            if (Session.GetInt32("UKPRN").HasValue)
            {
                Session.Remove("UKPRN");
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
                Providers = result.Items.Select(r => r.Record).Select(SearchProviderResultViewModel.FromProvider)
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
                Providers = result.Items.Select(r => r.Record).Select(SearchProviderResultViewModel.FromProvider)
            });
        }
    }
}
