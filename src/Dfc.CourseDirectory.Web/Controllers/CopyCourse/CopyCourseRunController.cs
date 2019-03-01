using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.CourseTextService;
using Dfc.CourseDirectory.Services.Interfaces.VenueService;
using Dfc.CourseDirectory.Services.VenueService;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.ViewModels;
using Dfc.CourseDirectory.Web.ViewModels.CopyCourse;
using Dfc.CourseDirectory.Web.ViewModels.EditCourse;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dfc.CourseDirectory.Web.Controllers.CopyCourse
{
    public class CopyCourseRunController : Controller
    {
        private readonly ILogger<CopyCourseRunController> _logger;
        private readonly IHttpContextAccessor _contextAccessor;

        private readonly ICourseService _courseService;

        private ISession _session => _contextAccessor.HttpContext.Session;
        private readonly IVenueSearchHelper _venueSearchHelper;
        private readonly IVenueService _venueService;

        public CopyCourseRunController(
            ILogger<CopyCourseRunController> logger,
            IOptions<CourseServiceSettings> courseSearchSettings,
            IHttpContextAccessor contextAccessor,
            ICourseService courseService,
            IVenueService venueService)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(courseSearchSettings, nameof(courseSearchSettings));
            Throw.IfNull(contextAccessor, nameof(contextAccessor));
            Throw.IfNull(courseService, nameof(courseService));
            Throw.IfNull(venueService, nameof(venueService));

            _logger = logger;
            _contextAccessor = contextAccessor;
            _courseService = courseService;
            _venueService = venueService;
        }


        [HttpGet]
        public async Task<IActionResult> Index(Guid? courseId, Guid courseRunId)
        {
            int? UKPRN;

            if (_session.GetInt32("UKPRN") != null)
            {
                UKPRN = _session.GetInt32("UKPRN").Value;
            }
            else
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            List<SelectListItem> courseRunVenues = new List<SelectListItem>();

            VenueSearchCriteria criteria = new VenueSearchCriteria(UKPRN.ToString(), null);
            var venues = await _venueService.SearchAsync(criteria);

            foreach (var venue in venues.Value.Value)
            {
                var item = new SelectListItem { Text = venue.VenueName, Value = venue.ID };
                courseRunVenues.Add(item);
            };

            var regions = _courseService.GetRegions();

            if (courseId.HasValue)
            {
                var course = await _courseService.GetCourseByIdAsync(new GetCourseByIdCriteria(courseId.Value));

                var courseRun = course.Value.CourseRuns.SingleOrDefault(cr => cr.id == courseRunId);

                if (courseRun != null)
                {
                    CopyCourseRunViewModel vm = new CopyCourseRunViewModel
                    {
                        CourseId = courseId.Value,
                        CourseRunId = courseRunId,
                        CourseName = courseRun?.CourseName,
                        Venues = courseRunVenues,
                        VenueId = courseRun.VenueId ?? Guid.Empty,
                        SelectRegion = regions,
                        DeliveryMode = courseRun.DeliveryMode,

                        CourseProviderReference = courseRun?.ProviderCourseID,
                        DurationUnit = courseRun.DurationUnit,
                        DurationLength = courseRun?.DurationValue?.ToString(),
                        StartDateType = courseRun.FlexibleStartDate
                            ? StartDateType.FlexibleStartDate
                            : StartDateType.SpecifiedStartDate,
                        Day = courseRun.StartDate?.Day.ToString("00"),
                        Month = courseRun.StartDate?.Month.ToString("00"),
                        Year = courseRun.StartDate?.Year.ToString("0000"),
                        StudyMode = courseRun.StudyMode,
                        Url = courseRun.CourseURL,
                        Cost = courseRun.Cost?.ToString("F"),
                        CostDescription = courseRun.CostDescription,
                        AttendanceMode = courseRun.AttendancePattern,
                        QualificationType = course.Value.QualificationType,
                        NotionalNVQLevelv2 = course.Value.NotionalNVQLevelv2
                    };

                    return View("CopyCourseRun", vm);
                }
            }

            //error page
            return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

        }

        [HttpPost]
        public async Task<IActionResult> Index(CopyCourseRunSaveViewModel model)
        {
            int? UKPRN;

            if (_session.GetInt32("UKPRN") != null)
            {
                UKPRN = _session.GetInt32("UKPRN").Value;
            }
            else
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            }

            if (model.CourseId.HasValue)
            {
                var courseForCopy = await _courseService.GetCourseByIdAsync(new GetCourseByIdCriteria(model.CourseId.Value));

                if (courseForCopy.IsSuccess && courseForCopy.HasValue)
                {
                    var regions = new List<string>();
                    var courseRunForCopy = courseForCopy.Value.CourseRuns.SingleOrDefault(cr => cr.id == model.CourseRunId);

                    courseRunForCopy.DurationUnit = model.DurationUnit;
                    courseRunForCopy.AttendancePattern = model.AttendanceMode;
                    courseRunForCopy.DeliveryMode = model.DeliveryMode;
                    courseRunForCopy.FlexibleStartDate = model.FlexibleStartDate;
                    courseRunForCopy.StudyMode = model.StudyMode;
                    courseRunForCopy.Cost = Convert.ToDecimal(model.Cost);
                    courseRunForCopy.CostDescription = model.CostDescription;
                    courseRunForCopy.CourseName = model.CourseName;
                    courseRunForCopy.CourseURL = model.Url;
                    courseRunForCopy.DurationValue = Convert.ToInt32(model.DurationLength);
                    courseRunForCopy.ProviderCourseID = model.CourseProviderReference;

                    bool flexibleStartDate = true;
                    DateTime? specifiedStartDate = null;

                    if (model.StartDateType.Equals("SpecifiedStartDate",
                        StringComparison.InvariantCultureIgnoreCase))
                    {
                        string day = model.Day.Length == 1 ? string.Concat("0", model.Day) : model.Day;
                        string month = model.Month.Length == 1 ? string.Concat("0", model.Month) : model.Month;
                        string startDate = string.Format("{0}-{1}-{2}", day, month, model.Year);
                        specifiedStartDate = DateTime.ParseExact(startDate, "dd-MM-yyyy",
                            System.Globalization.CultureInfo.InvariantCulture);

                        flexibleStartDate = false;
                    }

                    courseRunForCopy.FlexibleStartDate = flexibleStartDate;
                    courseRunForCopy.StartDate = specifiedStartDate;
                    courseRunForCopy.UpdatedDate = DateTime.Now;
                    courseRunForCopy.UpdatedBy = User.Identity.Name;

                    switch (model.DeliveryMode)
                    {
                        case DeliveryMode.ClassroomBased:

                            courseRunForCopy.Regions = null;
                            courseRunForCopy.VenueId = model.VenueId;
                            break;
                        case DeliveryMode.WorkBased:
                            courseRunForCopy.VenueId = null;


                            break;
                    }


                    var copiedCourse = await _courseService.UpdateCourseAsync(courseForCopy.Value);
                     if (copiedCourse.IsSuccess && copiedCourse.HasValue)
                    {
                        return RedirectToAction("Courses", "Provider",
                            new
                            {
                                level = courseForCopy.Value.NotionalNVQLevelv2,
                                courseId = copiedCourse.Value.id,
                                courseRunId = model.CourseRunId
                            });
                    }


                }
            }

            return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


    }
}