
using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.Spatial;
using Dfc.CourseDirectory.FindACourseApi.Models;


namespace Dfc.CourseDirectory.FindACourseApi.Interfaces
{
    public interface IAzureSearchCourseDetail //: ICourse
    {
        Course Course { get; set; }
        dynamic Provider { get; set; }
        dynamic Venue { get; set; }
        dynamic Qualification { get; set; }
    }
}
