
using System;
using Dfc.CourseDirectory.FindACourseApi.Models;


namespace Dfc.CourseDirectory.FindACourseApi.Interfaces
{
    public interface ICourseDetailResult
    {
        Course Course { get; set; }
        dynamic /*Provider*/ Provider { get; set; }
        dynamic /*Venue*/ Venue { get; set; }
        dynamic /*Qualification*/ Qualification { get; set; }
    }
}
