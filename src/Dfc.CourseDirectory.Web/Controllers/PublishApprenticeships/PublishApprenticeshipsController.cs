﻿using Dfc.CourseDirectory.Common;
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
using Dfc.CourseDirectory.Services.Interfaces.ApprenticeshipService;
using Dfc.CourseDirectory.Web.ViewModels.PublishApprenticeships;


namespace Dfc.CourseDirectory.Web.Controllers.PublishApprenticeships
{
    public class PublishApprenticeshipsController : Controller
    {
        private readonly ILogger<PublishApprenticeshipsController> _logger;
        private readonly IHttpContextAccessor _contextAccessor;
        private ISession _session => _contextAccessor.HttpContext.Session;
        private readonly IApprenticeshipService _apprenticeshipService;

    

         public PublishApprenticeshipsController(ILogger<PublishApprenticeshipsController> logger,
                IHttpContextAccessor contextAccessor, IApprenticeshipService apprenticeshipService)
         {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(apprenticeshipService, nameof(apprenticeshipService));
            _logger = logger;
            _contextAccessor = contextAccessor;
            _apprenticeshipService = apprenticeshipService;

         }


        [Authorize]
        [HttpGet]
        public IActionResult Index()
        {
            PublishApprenticeshipsViewModel vm = new PublishApprenticeshipsViewModel();
            int? UKPRN = _session.GetInt32("UKPRN");
            if (!UKPRN.HasValue)
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            var apprenticeships = _apprenticeshipService.GetApprenticeshipByUKPRN(UKPRN.Value.ToString()).Result.Value.Where(x => x.RecordStatus == RecordStatus.BulkUploadPending);

            vm.ListOfApprenticeships = apprenticeships;
            if (!apprenticeships.Any(x => x.RecordStatus == RecordStatus.BulkUploadPending))
            {
                vm.AreAllReadyToBePublished = true;
            }

            if (vm.AreAllReadyToBePublished)
            {
                return RedirectToAction("PublishYourFile", "BulkUploadApprenticeships", new { NumberOfApprenticeships = apprenticeships.SelectMany(s => s.ApprenticeshipLocations.Where(cr => cr.RecordStatus == RecordStatus.BulkUploadReadyToGoLive)).Count() });
            }
            return View("Index", vm);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Index(PublishApprenticeshipsViewModel vm)
        {
            PublishCompleteViewModel CompleteVM = new PublishCompleteViewModel();

            int? sUKPRN = _session.GetInt32("UKPRN");
            int UKPRN;
            if (!sUKPRN.HasValue)
                return RedirectToAction("Index", "Home", new { errmsg = "Please select a Provider." });
            else
                UKPRN = sUKPRN ?? 0;

            CompleteVM.NumberOfCoursesPublished = vm.NumberOfApprenticeships;
            await _apprenticeshipService.ChangeApprenticeshipStatusesForUKPRNSelection(UKPRN, (int)RecordStatus.MigrationPending, (int)RecordStatus.Archived);
            await _apprenticeshipService.ChangeApprenticeshipStatusesForUKPRNSelection(UKPRN, (int)RecordStatus.MigrationReadyToGoLive, (int)RecordStatus.Archived);

            //Archive any existing courses
            var resultArchivingCourses = await _apprenticeshipService.ChangeApprenticeshipStatusesForUKPRNSelection(UKPRN, (int)RecordStatus.Live, (int)RecordStatus.Archived);
            if (resultArchivingCourses.IsSuccess)
            {
                // Publish courses
                var resultPublishBulkUploadedCourses = await _apprenticeshipService.ChangeApprenticeshipStatusesForUKPRNSelection(UKPRN, (int)RecordStatus.BulkUploadReadyToGoLive, (int)RecordStatus.Live);
                CompleteVM.Mode = PublishMode.BulkUpload;
                if (resultPublishBulkUploadedCourses.IsSuccess)
                    return RedirectToAction("Complete", "Apprenticeships",new { CompleteVM });
                else
                    return RedirectToAction("Index", "Home", new { errmsg = "Publish All BulkUpload-PublishCourses Error" });

            }
            else
            {
                return RedirectToAction("Index", "Home", new { errmsg = "Publish All BulkUpload-ArchiveCourses Error" });
            }


        }


    } 
}
