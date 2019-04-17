using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
                CourseInstanceId = courseRun.CourseInstanceId,
                CourseName = courseRun.CourseName,
                VenueId = courseRun.VenueId,
                CourseURL = courseRun.CourseURL,
                Cost = courseRun.Cost,
                CostDescription = courseRun.CostDescription,
                DurationUnit = courseRun.DurationUnit,
                DurationValue = courseRun.DurationValue,
                ProviderCourseID = courseRun.ProviderCourseID,
                DeliveryMode = courseRun.DeliveryMode,
                FlexibleStartDate = courseRun.FlexibleStartDate,
                StartDate = courseRun.StartDate,
                UpdatedDate = courseRun.UpdatedDate,
                UpdatedBy = courseRun.UpdatedBy
            };
            if(vm.VenueId != null)
            {
                vm.VenueName = _venueService.GetVenueByIdAsync(new GetVenueByIdCriteria(courseRun.VenueId.Value.ToString())).Result.Value.VenueName;
            }
            return View(vm);
        }
    }
}
