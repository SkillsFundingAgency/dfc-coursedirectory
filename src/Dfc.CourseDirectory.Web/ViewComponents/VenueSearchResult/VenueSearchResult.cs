using Dfc.CourseDirectory.Web.ViewComponents.VenueSearchResult;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Dfc.CourseDirectory.Web.ViewComponents.VenueSearchResult
{
    public class VenueSearchResult : ViewComponent
    {

        private readonly IHttpContextAccessor _contextAccessor;
        private ISession _session => _contextAccessor.HttpContext.Session;



        public VenueSearchResult(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;

            _contextAccessor.HttpContext.Session.SetString("UKPRN", "MyStore");
            _session.SetString("Test", "Ben Rules!");
        }

        public IViewComponentResult Invoke(VenueSearchResultModel model)
        {
            _session.SetString("Test", "Ben Rules!");
            if (model != null)
            {
                TempData["UKPRN"] = model.SearchTerm;
                TempData.Keep();

                _contextAccessor.HttpContext.Session.SetString("UKPRN", model.SearchTerm);

                var a= _contextAccessor.HttpContext.Session.GetString("UKPRN");
            }

            var b = _contextAccessor.HttpContext.Session.GetString("UKPRN");

            _session.GetString("Test");

            var actualModel = model ?? new VenueSearchResultModel();

            return View("~/ViewComponents/VenueSearchResult/Default.cshtml", actualModel);
        }
    }
}
