﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Models.Interfaces.Courses;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Models.Models.Regions;
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
                FlexibleStartDate = courseRun.FlexibleStartDate,
                StudyMode = courseRun.StudyMode,
                AttendancePattern = courseRun.AttendancePattern,
                StartDate = courseRun.StartDate,
                UpdatedDate = courseRun.UpdatedDate,
                UpdatedBy = courseRun.UpdatedBy
            };
            if(vm.VenueId != null)
            {
                vm.VenueName = _venueService.GetVenueByIdAsync(new GetVenueByIdCriteria(courseRun.VenueId.Value.ToString())).Result.Value.VenueName;
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
            if(courseRun.Regions != null)
            {
                var allRegions = _courseService.GetRegions().RegionItems;
                var regions = GetRegions().RegionItems.Select(x => x.Id);
                vm.Regions = FormattedRegionsByIds(allRegions, courseRun.Regions);
            }
            return View(vm);
        }
        internal IEnumerable<string> FormattedRegionsByIds(IEnumerable<RegionItemModel> list, IEnumerable<string> ids)
        {
            if (list == null) list = new List<RegionItemModel>();
            if (ids == null) ids = new List<string>();

            var regionNames = (from regionItemModel in list
                               from subRegionItemModel in regionItemModel.SubRegion
                               where ids.Contains(subRegionItemModel.Id)
                               select regionItemModel.RegionName).Distinct().OrderBy(x => x);

            return regionNames;
        }
        private SelectRegionModel GetRegions()
        {
            var selectRegion = new SelectRegionModel
            {
                LabelText = "Select course region",
                HintText = "For example, South West",
                AriaDescribedBy = "Select all that apply."
            };

            if (selectRegion.RegionItems != null && selectRegion.RegionItems.Any())
            {
                selectRegion.RegionItems = selectRegion.RegionItems.OrderBy(x => x.RegionName);
                foreach (var selectRegionRegionItem in selectRegion.RegionItems)
                {
                    selectRegionRegionItem.SubRegion = selectRegionRegionItem.SubRegion.OrderBy(x => x.SubRegionName).ToList();
                }
            }

            return selectRegion;
        }
    }

}
