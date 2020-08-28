using System;
using System.Linq;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Interfaces.Courses;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.VenueService;
using Dfc.CourseDirectory.Services.VenueService;
using Dfc.CourseDirectory.Web.ViewModels.CourseSummary;
using Microsoft.AspNetCore.Mvc;


namespace Dfc.CourseDirectory.Web.Controllers
{
    public class CourseSummaryController : Controller
    {
        private readonly ICourseService _courseService;
        private readonly IVenueService _venueService;

        public CourseSummaryController(
            ICourseService courseService,
            IVenueService venueService
            )
        {
            Throw.IfNull(courseService, nameof(courseService));
            Throw.IfNull(venueService, nameof(venueService));
            _courseService = courseService;
            _venueService = venueService;
        }
        public IActionResult Index(Guid? courseId, Guid? courseRunId)
        {
            ICourse course = null;
            CourseRun courseRun = null;
            if (courseId.HasValue)
            {
                course = _courseService.GetCourseByIdAsync(new GetCourseByIdCriteria(courseId.Value)).Result.Value;
                courseRun = course.CourseRuns.Where(x => x.id == courseRunId.Value).FirstOrDefault();
            }

            CourseSummaryViewModel vm = new CourseSummaryViewModel

            {
                ProviderUKPRN = course.ProviderUKPRN,
                CourseId = course.id,
                QualificationCourseTitle = course.QualificationCourseTitle,
                LearnAimRef = course.LearnAimRef,
                NotionalNVQLevelv2 = course.NotionalNVQLevelv2,
                AwardOrgCode = course.AwardOrgCode,
                CourseDescription = course.CourseDescription,
                EntryRequirements = course.EntryRequirements,
                WhatYoullLearn = course.WhatYoullLearn,
                HowYoullLearn = course.HowYoullLearn,
                WhatYoullNeed = course.WhatYoullNeed,
                HowYoullBeAssessed = course.HowYoullBeAssessed,
                WhereNext = course.WhereNext,
                IsValid = course.IsValid,
                QualificationType = course.QualificationType,

                //Course run deets
                CourseInstanceId = courseRunId,
                CourseName = courseRun.CourseName,
                VenueId = courseRun.VenueId,
                Cost = courseRun.Cost,
                CostDescription = courseRun.CostDescription,
                DurationUnit = courseRun.DurationUnit,
                DurationValue = courseRun.DurationValue,
                ProviderCourseID = courseRun.ProviderCourseID,
                DeliveryMode = courseRun.DeliveryMode,
                National = courseRun.DeliveryMode == DeliveryMode.WorkBased & !courseRun.National.HasValue ||
                           courseRun.National.GetValueOrDefault(),
                FlexibleStartDate = courseRun.FlexibleStartDate,
                StartDate = courseRun.StartDate,
                StudyMode = courseRun.StudyMode,
                AttendancePattern = courseRun.AttendancePattern,
                CreatedBy = courseRun.CreatedBy,
                CreatedDate = courseRun.CreatedDate,

            };

            //Determine newer edited date
            if (course.UpdatedDate > courseRun.UpdatedDate)
            {
                vm.UpdatedDate = course.UpdatedDate;
                vm.UpdatedBy = course.UpdatedBy;
            }
            else
            {
                vm.UpdatedDate = courseRun.UpdatedDate;
                vm.UpdatedBy = courseRun.UpdatedBy;
            }

            if (vm.VenueId != null)
            {
                if (vm.VenueId != Guid.Empty)
                {
                    vm.VenueName = _venueService
                        .GetVenueByIdAsync(new GetVenueByIdCriteria(courseRun.VenueId.Value.ToString())).Result.Value
                        .VenueName;

                }
            }

            if(!string.IsNullOrEmpty(courseRun.CourseURL))
            {
                if (courseRun.CourseURL.Contains("http") || courseRun.CourseURL.Contains("https"))
                {
                    vm.CourseURL = courseRun.CourseURL;
                }
                else
                {
                    vm.CourseURL = "http://" + courseRun.CourseURL;
                }
            }

            if (courseRun.Regions != null)
            {
                var availableRegions = _courseService.GetRegions();
                var regionIds = availableRegions.SubRegionsDataCleanse(courseRun.Regions.ToList());

                var availableRegionNames = availableRegions.RegionItems
                    .Select(r => new { r.Id, r.RegionName })
                    .Concat(availableRegions.RegionItems
                        .SelectMany(r => r.SubRegion)
                        .Select(r => new { r.Id, RegionName = r.SubRegionName }));

                vm.Regions = availableRegionNames
                    .Where(r => regionIds.Contains(r.Id))
                    .Select(r => r.RegionName);
            }

            return View(vm);
        }
    }
}