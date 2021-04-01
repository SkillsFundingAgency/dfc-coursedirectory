using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.WebV2;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Dfc.CourseDirectory.Web.ViewComponents.Apprenticeships
{
    public class ChooseLocation : ViewComponent
    {
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly IProviderContextProvider _providerContextProvider;

        private ISession Session => HttpContext.Session;

        public ChooseLocation(ISqlQueryDispatcher sqlQueryDispatcher, IProviderContextProvider providerContextProvider)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _providerContextProvider = providerContextProvider;
        }

        public async Task<IViewComponentResult> InvokeAsync(ChooseLocationModel model)
        {
            var providerId = _providerContextProvider.GetProviderId(withLegacyFallback: true);

            List<SelectListItem> venues = new List<SelectListItem>();

            var result = await _sqlQueryDispatcher.ExecuteQuery(new GetVenuesByProvider() { ProviderId = providerId });

            foreach (var venue in result)
            {
                var item = new SelectListItem { Text = venue.VenueName, Value = venue.VenueId.ToString() };

                if (model.DeliveryLocations == null || !model.DeliveryLocations.Any(x => x.LocationId.HasValue && x.LocationId.Value.ToString() == item.Value))
                {
                    venues.Add(item);
                }
            };

            model.Locations = venues;
            return View("~/ViewComponents/Apprenticeships/ChooseLocation/Default.cshtml", model);

        }
    }
}
