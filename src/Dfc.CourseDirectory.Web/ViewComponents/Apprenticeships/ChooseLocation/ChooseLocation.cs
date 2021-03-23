using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Dfc.CourseDirectory.Web.ViewComponents.Apprenticeships
{
    public class ChooseLocation : ViewComponent
    {
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;

        private ISession Session => HttpContext.Session;

        public ChooseLocation(ICosmosDbQueryDispatcher cosmosDbQueryDispatcher)
        {
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
        }

        public async Task<IViewComponentResult> InvokeAsync(ChooseLocationModel model)
        {
            List<SelectListItem> venues = new List<SelectListItem>();

            var UKPRN = Session.GetInt32("UKPRN");
            if (UKPRN.HasValue)
            {
                var result = await _cosmosDbQueryDispatcher.ExecuteQuery(new GetVenuesByProvider() { ProviderUkprn = UKPRN.Value });

                foreach (var venue in result)
                {
                    var item = new SelectListItem { Text = venue.VenueName, Value = venue.Id.ToString() };

                    if (model.DeliveryLocations == null || !model.DeliveryLocations.Any(x => x.LocationId.HasValue && x.LocationId.Value.ToString() == item.Value))
                    {
                        venues.Add(item);
                    }
                };

            }

            model.Locations = venues;
            return View("~/ViewComponents/Apprenticeships/ChooseLocation/Default.cshtml", model);

        }
    }
}
