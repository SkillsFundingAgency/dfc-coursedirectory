using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Services;
using Dfc.CourseDirectory.Services.Interfaces.VenueService;
using Dfc.CourseDirectory.Services.VenueService;
using Dfc.CourseDirectory.Web.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Models.Models.Venues;
using Dfc.CourseDirectory.Web.ViewModels.Apprenticeships;

namespace Dfc.CourseDirectory.Web.ViewComponents.Apprenticeships
{
    public class DeliveryOptionsList : ViewComponent
    {
        public IViewComponentResult Invoke(DeliveryOptionSummary model)
        {
            return View("~/ViewComponents/Apprenticeships/DeliveryOptionsList/Default.cshtml", model);

        }
    }
}
