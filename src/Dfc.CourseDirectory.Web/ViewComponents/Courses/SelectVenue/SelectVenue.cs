using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services.Interfaces.VenueService;
using Dfc.CourseDirectory.Services.VenueService;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.RequestModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.SelectVenue
{
    public class SelectVenue : ViewComponent
    {
        private readonly IVenueSearchHelper _venueSearchHelper;
        private readonly IVenueService _venueService;
        private readonly IVenueServiceSettings _venueServiceSettings;

        public SelectVenue(IVenueSearchHelper venueSearchHelper, IVenueService venueService, IOptions<VenueServiceSettings> venueServiceSettings)
        {
            Throw.IfNull(venueService, nameof(venueService));
            Throw.IfNull(venueServiceSettings, nameof(venueServiceSettings));

            _venueServiceSettings = venueServiceSettings.Value;
            _venueSearchHelper = venueSearchHelper;
            _venueService = venueService;
        }
        public async Task<IViewComponentResult> InvokeAsync(SelectVenueModel model)
        {
            // do some gubbins here to retrieve venue
            var requestModel = new VenueSearchRequestModel { SearchTerm = model.Ukprn.ToString() };
            var criteria = _venueSearchHelper.GetVenueSearchCriteria(requestModel);
            var result = await _venueService.SearchAsync(criteria);

            if (result.IsSuccess && result.HasValue)
            {
                var items = _venueSearchHelper.GetVenueSearchResultItemModels(result.Value.Value);
                var venueItems = new List<VenueItemModel>();
                foreach (var venueSearchResultItemModel in items)
                {
                    venueItems.Add(new VenueItemModel
                    {
                        Id = venueSearchResultItemModel.Id,
                        VenueName = venueSearchResultItemModel.VenueName
                    });
                }

                model.VenueItems = venueItems;
            }
            return View("~/ViewComponents/Courses/SelectVenue/Default.cshtml", model);
        }
    }
}