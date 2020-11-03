using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Services.Enums;
using Dfc.CourseDirectory.Services.Models.Courses;
using Dfc.CourseDirectory.Services.Models.Regions;

namespace Dfc.CourseDirectory.Web.ViewModels.PublishCourses
{
    public class PublishViewModel
    {
        public IEnumerable<Course> Courses { get; set; }

        public Dictionary<Guid, string> Venues { get; set; }
        public int NumberOfCoursesInFiles { get; set; }

        public PublishMode PublishMode { get; set; }

        public bool AreAllReadyToBePublished { get; set; }

        public string NotificationTitle { get; set; }
        public string NotificationMessage { get; set; }

        public Guid? CourseId { get; set; }

        public Guid? CourseRunId { get; set; }
        public IEnumerable<RegionItemModel> Regions { get; set; }
    }
}
