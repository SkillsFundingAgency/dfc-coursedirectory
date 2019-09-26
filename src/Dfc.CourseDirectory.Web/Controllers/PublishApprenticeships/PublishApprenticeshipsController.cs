using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.BlobStorageService;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.VenueService;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.ViewModels.BulkUpload;
using Dfc.CourseDirectory.Web.ViewModels.PublishCourses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Dfc.CourseDirectory.Web.Controllers.PublishApprenticeships
{
    public class PublishApprenticeshipsController : Controller
    {
        private readonly ILogger<PublishApprenticeshipsController> _logger;
        private readonly IHttpContextAccessor _contextAccessor;
        private ISession _session => _contextAccessor.HttpContext.Session;
        private readonly ICourseService _courseService;
        private readonly IVenueService _venueService;
        private readonly IBlobStorageService _blobStorageService;
    

     public PublishApprenticeshipsController(ILogger<PublishApprenticeshipsController> logger,
            IHttpContextAccessor contextAccessor, ICourseService courseService,
            IVenueService venueService,IBlobStorageService blobStorageService)
    {
        Throw.IfNull(logger, nameof(logger));
        Throw.IfNull(courseService, nameof(courseService));
        Throw.IfNull(venueService, nameof(venueService));
        Throw.IfNull(blobStorageService, nameof(blobStorageService));
        _logger = logger;
        _contextAccessor = contextAccessor;
        _courseService = courseService;
        _venueService = venueService;
        _blobStorageService = blobStorageService;
    }


    [Authorize]
    [HttpGet]
    public IActionResult Index(PublishMode publishMode, string notificationTitle, Guid? courseId, Guid? courseRunId, bool fromBulkUpload)
    {
        int? UKPRN = _session.GetInt32("UKPRN");
        if (!UKPRN.HasValue)
            return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });

        List<Course> Courses = new List<Course>();
        ICourseSearchResult coursesByUKPRN = (!UKPRN.HasValue
                ? null
                : _courseService.GetYourCoursesByUKPRNAsync(new CourseSearchCriteria(UKPRN))
                    .Result.Value);
        
        

        return View("Index");
    }


} 
}
