using Dfc.CourseDirectory.Services;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Web.ViewModels.ProviderCourses
{
    public class ProviderCoursesFilterItemModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Text { get; set; }
        public string Value { get; set; }
        public int Count { get; set; }
        public bool IsSelected { get; set; }

       
    }
}