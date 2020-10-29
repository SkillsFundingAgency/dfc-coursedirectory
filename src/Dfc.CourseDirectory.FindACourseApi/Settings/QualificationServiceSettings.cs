
using System;
using Dfc.CourseDirectory.FindACourseApi.Interfaces;


namespace Dfc.CourseDirectory.FindACourseApi.Settings
{
    public class QualificationServiceSettings : IQualificationServiceSettings
    {
        public string SearchService { get; set; }
        public string QueryKey { get; set; }
        public string AdminKey { get; set; }
        public string Index { get; set; }
    }
}
