using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Services.Interfaces.VenueService;
using Dfc.CourseDirectory.Services.VenueService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.CourseRun
{
    public class CourseRun : ViewComponent
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private ISession _session => _contextAccessor.HttpContext.Session;
        private IVenueService _service;

        public CourseRun(
            IHttpContextAccessor contextAccessor, IVenueService venueService)
        {
            Throw.IfNull(contextAccessor, nameof(contextAccessor));
            Throw.IfNull(venueService, nameof(venueService));

            _contextAccessor = contextAccessor;
            _service = venueService;

        }
        public async Task<IViewComponentResult> InvokeAsync(Dfc.CourseDirectory.Models.Models.Courses.CourseRun model)
        {
            var courseRunVenues = new List<SelectListItem>();
            var deliveryModes = new List<SelectListItem>();
            var durationUnits = new List<SelectListItem>();
            var attendances = new List<SelectListItem>();
            var modes = new List<SelectListItem>();

            var UKPRN = _session.GetInt32("UKPRN");
            if (UKPRN.HasValue)
            {
                VenueSearchCriteria criteria = new VenueSearchCriteria(UKPRN.ToString(), null);

                var venues = await _service.SearchAsync(criteria);

                foreach (var venue in venues.Value.Value)
                {
                    var item = new SelectListItem
                        { Text = venue.VenueName, Value = venue.ID };

                    if (model.VenueId.ToString() == venue.ID)
                    {
                        item.Selected = true;
                    }

                    courseRunVenues.Add(item);
                };

            }

            foreach (DeliveryMode eVal in DeliveryMode.GetValues(typeof(DeliveryMode)))
            {
                if (eVal.ToString().ToUpper() != "UNDEFINED")
                {
                    var item = new SelectListItem
                    { Text = System.Enum.GetName(typeof(DeliveryMode), eVal), Value = eVal.ToString() };

                    if (model.DeliveryMode.ToString() == eVal.ToString())
                    {
                        item.Selected = true;
                    }

                    deliveryModes.Add(item);
                }
            };

            foreach (DurationUnit eVal in DurationUnit.GetValues(typeof(DurationUnit)))
            {
                if (eVal.ToString().ToUpper() != "UNDEFINED")
                {
                    var item = new SelectListItem
                    { Text = System.Enum.GetName(typeof(DurationUnit), eVal), Value = eVal.ToString() };

                    if (model.DurationUnit.ToString() == eVal.ToString())
                    {
                        item.Selected = true;
                    }

                    durationUnits.Add(item);
                }
            };

            foreach (AttendancePattern eVal in AttendancePattern.GetValues(typeof(AttendancePattern)))
            {
                if (eVal.ToString().ToUpper() != "UNDEFINED")
                {
                    var item = new SelectListItem
                    { Text = System.Enum.GetName(typeof(AttendancePattern), eVal), Value = eVal.ToString() };

                    if (model.AttendancePattern.ToString() == eVal.ToString())
                    {
                        item.Selected = true;
                    }

                    attendances.Add(item);
                }
            };

            foreach (Dfc.CourseDirectory.Models.Models.Courses.StudyMode eVal in Dfc.CourseDirectory.Models.Models.Courses.StudyMode.GetValues(typeof(Dfc.CourseDirectory.Models.Models.Courses.StudyMode)))
            {
                if (eVal.ToString().ToUpper() != "UNDEFINED")
                {
                    var item = new SelectListItem
                    { Text = System.Enum.GetName(typeof(Dfc.CourseDirectory.Models.Models.Courses.StudyMode), eVal), Value = eVal.ToString() };

                    if (model.StudyMode.ToString() == eVal.ToString())
                    {
                        item.Selected = true;
                    }

                    modes.Add(item);
                }
            };




            CourseRunModel courseRunModel = new CourseRunModel()
            {
                VenueId = model.VenueId,
                venues = courseRunVenues,
                courseRun = model,
                deliveryModes = deliveryModes,
                durationUnits = durationUnits,
                attendances = attendances,
                modes = modes,
                deliveryMode = model.DeliveryMode,
                durationUnit = model.DurationUnit,
                attendance = model.AttendancePattern,
                mode = model.StudyMode

            };



            return View("~/ViewComponents/Courses/CourseRun/Default.cshtml", courseRunModel);
        }
    }
}
