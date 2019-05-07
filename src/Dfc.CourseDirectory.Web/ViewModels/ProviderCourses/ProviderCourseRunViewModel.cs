using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewModels.ProviderCourses
{
    public class ProviderCourseRunViewModel
    {
        public Guid? CourseId { get; set; }
        public string QualificationCourseTitle { get; set; }
        public string LearnAimRef { get; set; }
        public string NotionalNVQLevelv2 { get; set; }
        public string AwardOrgCode { get; set; }
        public string QualificationType { get; set; }


        public string CourseTextId { get; set; }


        public string CourseRunId { get; set; }
        public string CourseName { get; set; }
        public string StartDate { get; set; }
        public string Url { get; set; }
        public string Cost { get; set; }
        public string Duration { get; set; }
        public string Region { get; set; }
        public string Venue { get; set; }
        public string DeliveryMode { get; set; }
        public string StudyMode { get; set; }
        public string AttendancePattern { get; set; }
        public IEnumerable<string> Regions { get; set; }

        public string RegionIdList { get; set; }

        public Guid? VenueId { get; set; }
        public bool National { get; set; }
    }
}


