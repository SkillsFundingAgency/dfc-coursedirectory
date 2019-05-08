using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.VenueService;
using Dfc.CourseDirectory.Services.VenueService;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.RequestModels;
using Dfc.CourseDirectory.Web.ViewComponents.Apprenticeships.ApprenticeshipSearchResult;
using Dfc.CourseDirectory.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.Web.Controllers
{
    public class ApprenticeshipsController : Controller
    {
        private readonly ILogger<ApprenticeshipsController> _logger;
        private readonly IHttpContextAccessor _contextAccessor;
        private ISession _session => _contextAccessor.HttpContext.Session;
        private readonly ICourseService _courseService;
        private readonly IVenueService _venueService;


        public ApprenticeshipsController(
            ILogger<ApprenticeshipsController> logger,
            IHttpContextAccessor contextAccessor, ICourseService courseService, IVenueService venueService)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(contextAccessor, nameof(contextAccessor));
            Throw.IfNull(courseService, nameof(courseService));
            Throw.IfNull(venueService, nameof(venueService));

            _logger = logger;
            _contextAccessor = contextAccessor;
            _courseService = courseService;
            _venueService = venueService;
        }
        [Authorize]
        public IActionResult Index()
        {
            //_session.SetString("Option", "Qualifications");
            return View();
        }


        [Authorize]
        public async Task<IActionResult> ApprenticeshipSearch([FromQuery] ApprenticeShipSearchRequestModel requestModel)
        {
            ApprenticeshipsSearchResultModel model = new ApprenticeshipsSearchResultModel();

            //if (requestModel == null)
            //{
            //    model = new LarsSearchResultModel();
            //}
            //else
            //{
            //    var criteria = _larsSearchHelper.GetLarsSearchCriteria(
            //        requestModel,
            //        _paginationHelper.GetCurrentPageNo(Request.GetDisplayUrl(), _larsSearchSettings.PageParamName),
            //        _larsSearchSettings.ItemsPerPage,
            //        (LarsSearchFacet[])Enum.GetValues(typeof(LarsSearchFacet)));

            //    var result = await _larsSearchService.SearchAsync(criteria);
            //    if (result.IsSuccess && result.HasValue) // && result.Value.Value.Count() > 0)
            //    {
            //        var filters = _larsSearchHelper.GetLarsSearchFilterModels(result.Value.SearchFacets, requestModel);
            //        var items = _larsSearchHelper.GetLarsSearchResultItemModels(result.Value.Value);

            //        model = new LarsSearchResultModel(
            //            requestModel.SearchTerm,
            //            items,
            //            Request.GetDisplayUrl(),
            //            _larsSearchSettings.PageParamName,
            //            _larsSearchSettings.ItemsPerPage,
            //            result.Value.ODataCount ?? 0,
            //            filters);
            //    }
            //    else
            //    {
            //        model = new LarsSearchResultModel(result.Error);
            //    }
            //}
            //_logger.LogMethodExit();
            return ViewComponent(nameof(ViewComponents.Apprenticeships.ApprenticeshipSearchResult.ApprenticeshipSearchResult), model);
        }


        //[Authorize]
        //public async Task<IActionResult> QualificationsList()
        //{
        //    var qualificationTypes = new List<string>();

        //    var providerUKPRN = User.Claims.SingleOrDefault(x => x.Type == "UKPRN");
        //    if (providerUKPRN != null)
        //    {
        //        _session.SetInt32("UKPRN", Int32.Parse(providerUKPRN.Value));
        //    }

        //    var UKPRN = _session.GetInt32("UKPRN");

        //    List<QualificationViewModel> qualificationsList = new List<QualificationViewModel>();

        //    if (UKPRN.HasValue)
        //    {
        //        QualificationViewModel qualification = new QualificationViewModel();

        //        var coursesByUKPRN = !UKPRN.HasValue
        //            ? null
        //            : _courseService.GetYourCoursesByUKPRNAsync(new CourseSearchCriteria(UKPRN))
        //                .Result.Value;

        //        IActionResult view = await GetCoursesViewModelAsync("", "", "", "", null);
        //        CoursesViewModel vm = (CoursesViewModel)(((ViewResult)view).Model);

        //        IEnumerable<CoursesForQualificationAndCountViewModel> coursesForQualifcationsWithCourseRunsCount = vm.Courses.Value?
        //            .Select(c => new CoursesForQualificationAndCountViewModel
        //            {
        //                QualificationType = c.QualType,
        //                CourseRunCount = c.Value.SelectMany(d => d.Value.SelectMany(g => g.CourseRuns)).Count(),
        //            }).ToList();

        //        return View(coursesForQualifcationsWithCourseRunsCount);
        //    }

        //    return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });

        //}

        //[Authorize]
        //private async Task<IActionResult> GetCoursesViewModelAsync(string status, string learnAimRef,
        //    string numberOfNewCourses, string errmsg, Guid? updatedCourseId)
        //{
        //    if (!string.IsNullOrEmpty(status))
        //    {
        //        ViewData["Status"] = status;
        //        switch (status.ToUpper())
        //        {
        //            case "GOOD":
        //                ViewData["StatusMessage"] =
        //                    string.Format("{0} New Course(s) created in Course Directory for LARS: {1}",
        //                        numberOfNewCourses, learnAimRef);
        //                break;
        //            case "BAD":
        //                ViewData["StatusMessage"] = errmsg;
        //                break;
        //            case "UPDATE":
        //                ViewData["StatusMessage"] = string.Format("Course run updated in Course Directory");
        //                break;
        //            default:
        //                break;
        //        }
        //    }

        //    int? UKPRN = _session.GetInt32("UKPRN");

        //    if (!UKPRN.HasValue)
        //    {
        //        return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
        //    }

        //    ICourseSearchResult result = (!UKPRN.HasValue
        //        ? null
        //        : _courseService.GetYourCoursesByUKPRNAsync(new CourseSearchCriteria(UKPRN))
        //            .Result.Value);


        //    CoursesViewModel vm = new CoursesViewModel
        //    {
        //        UKPRN = UKPRN,
        //        Courses = result,
        //    };

        //    return View(vm);
        //}


    }
}