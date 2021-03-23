using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.ChooseVenue
{
    public class ChooseVenue : ViewComponent
    {
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;

        private ISession Session => HttpContext.Session;

        public ChooseVenue(ICosmosDbQueryDispatcher cosmosDbQueryDispatcher)
        {
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
        }

        public async Task<IViewComponentResult> InvokeAsync(ChooseVenueModel model)
        {
            List<SelectListItem> venues = new List<SelectListItem>();

            var UKPRN = Session.GetInt32("UKPRN");
            if (UKPRN.HasValue)
            {
                var result = await _cosmosDbQueryDispatcher.ExecuteQuery(new GetVenuesByProvider() { ProviderUkprn = UKPRN.Value });

                var defaultItem = new SelectListItem { Text = "Select", Value = "" };

                foreach (var venue in result)
                {
                    var item = new SelectListItem { Text = venue.VenueName, Value = venue.Id.ToString() };
                    venues.Add(item);
                };

                venues.Insert(0, defaultItem);
            }

            model.Venues = venues;
            return View("~/ViewComponents/Courses/ChooseVenue/Default.cshtml", model);

        }
    }
}
