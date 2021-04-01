using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Web.ViewComponents.VenueSearchResult;
using Dfc.CourseDirectory.WebV2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class VenueSearchController : Controller
    {
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly IProviderContextProvider _providerContextProvider;

        public VenueSearchController(
            ISqlQueryDispatcher sqlQueryDispatcher,
            IProviderContextProvider providerContextProvider)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _providerContextProvider = providerContextProvider;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var providerContext = _providerContextProvider.GetProviderContext(withLegacyFallback: true);

            var venues = await _sqlQueryDispatcher.ExecuteQuery(new GetVenuesByProvider() { ProviderId = providerContext.ProviderInfo.ProviderId });

            var items = venues.Select(v => new VenueSearchResultItemModel(
                v.VenueName,
                v.AddressLine1,
                v.AddressLine2,
                v.Town,
                v.County,
                v.Postcode,
                v.VenueId.ToString()));

            var model = new VenueSearchResultModel(
                searchTerm: providerContext.ProviderInfo.Ukprn.ToString(),
                items,
                newItem: null,
                updated: false);

            return ViewComponent(nameof(ViewComponents.VenueSearchResult.VenueSearchResult), model);
        }
    }
}
