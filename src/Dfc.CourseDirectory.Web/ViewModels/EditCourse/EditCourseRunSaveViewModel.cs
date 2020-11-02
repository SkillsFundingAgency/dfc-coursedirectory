using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Services.Enums;
using Dfc.CourseDirectory.Services.Models;
using Dfc.CourseDirectory.Services.Models.Courses;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Dfc.CourseDirectory.Web.ViewModels.EditCourse
{
    public class EditCourseRunSaveViewModel
    {
        public string CourseName { get; set; }
        public DateTime StartDate { get; set; }
        public string StartDateType { get; set; }
        public string Day { get; set; }
        public string Month { get; set; }
        public string Year { get; set; }
        public string DurationLength { get; set; }
        public string Cost { get; set; }
        public string CostDescription { get; set; }

        public Guid  VenueId { get; set; }
        public bool National { get; set; }
        public string[] SelectedRegions { get; set; }
        public DeliveryMode DeliveryMode { get; set; }
        public string CourseProviderReference { get; set; }
        public string Url { get; set; }
        public DurationUnit DurationUnit { get; set; }
        public StudyMode StudyMode { get; set; }
        public AttendancePattern AttendanceMode { get; set; }
        public Guid? CourseId { get; set; }
        public Guid CourseRunId { get; set; }

        public string QualificationType { get; set; }

        public bool FlexibleStartDate { get; set; }

        public PublishMode Mode { get; set; }

    }

}
