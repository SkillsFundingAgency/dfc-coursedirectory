

using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Services.Models.Courses;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Dfc.CourseDirectory.Web.ViewComponents.Courses.CourseRun
{
    public class CourseRunModel
    {
        public bool Readonly { get; set; }

        public Guid CourseId { get; set; }
        public Dfc.CourseDirectory.Services.Models.Courses.CourseRun courseRun { get; set; }

        public List<SelectListItem> deliveryModes { get; set; }

        public List<SelectListItem> durationUnits { get; set; }

        public List<SelectListItem> attendances { get; set; }
        public List<SelectListItem> modes { get; set; }

        public List<SelectListItem> venues { get; set; }

        //public Guid? VenueId { get; set; }
        //public DeliveryMode deliveryMode { get; set; }

        //public DurationUnit durationUnit { get; set; }
        //public AttendancePattern attendance { get; set; }
        //public Dfc.CourseDirectory.Models.Models.Courses.StudyMode mode { get; set; }
    }
}
