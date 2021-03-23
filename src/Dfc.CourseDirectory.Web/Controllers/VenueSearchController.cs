using System;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Web.ViewComponents.VenueSearchResult;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class VenueSearchController : Controller
    {
        private readonly ILogger<VenueSearchController> _logger;
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private ISession Session => HttpContext.Session;

        public VenueSearchController(
            ILogger<VenueSearchController> logger,
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            _logger = logger;
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            int UKPRN = 0;
            if (Session.GetInt32("UKPRN").HasValue)
            {
                UKPRN = Session.GetInt32("UKPRN").Value;
            }
            else
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            var venues = await _cosmosDbQueryDispatcher.ExecuteQuery(new GetVenuesByProvider() { ProviderUkprn = UKPRN });

            var items = venues.Select(v => new VenueSearchResultItemModel(
                v.VenueName,
                v.AddressLine1,
                v.AddressLine2,
                v.Town,
                v.County,
                v.Postcode,
                v.Id.ToString()));

            var model = new VenueSearchResultModel(
                searchTerm: UKPRN.ToString(),
                items,
                newItem: null,
                updated: false);

            return ViewComponent(nameof(ViewComponents.VenueSearchResult.VenueSearchResult), model);
        }
    }
}
