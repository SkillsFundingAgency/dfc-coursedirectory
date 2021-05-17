using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.WebV2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.ChooseVenue
{
    public class ChooseVenue : ViewComponent
    {
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly IProviderContextProvider _providerContextProvider;

        public ChooseVenue(ISqlQueryDispatcher sqlQueryDispatcher, IProviderContextProvider providerContextProvider)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _providerContextProvider = providerContextProvider;
        }

        public async Task<IViewComponentResult> InvokeAsync(ChooseVenueModel model)
        {
            var providerId = _providerContextProvider.GetProviderId(withLegacyFallback: true);

            List<SelectListItem> venues = new List<SelectListItem>();

            var result = await _sqlQueryDispatcher.ExecuteQuery(new GetVenuesByProvider() { ProviderId = providerId });

            var defaultItem = new SelectListItem { Text = "Select", Value = "" };

            foreach (var venue in result)
            {
                var item = new SelectListItem { Text = venue.VenueName, Value = venue.VenueId.ToString() };
                venues.Add(item);
            };

            venues.Insert(0, defaultItem);

            model.Venues = venues;
            return View("~/ViewComponents/Courses/ChooseVenue/Default.cshtml", model);

        }
    }
}
