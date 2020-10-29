
using System;
using Dfc.CourseDirectory.FindACourseApi.Interfaces;


namespace Dfc.CourseDirectory.FindACourseApi.Settings
{
    public class CourseServiceSettings : ICourseServiceSettings
    {
        public string ApiUrl { get; set; }
        public string ApiKey { get; set; }
    }
}
