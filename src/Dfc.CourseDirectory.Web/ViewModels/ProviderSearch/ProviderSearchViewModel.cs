
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Dfc.CourseDirectory.Models.Models.Providers;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;


namespace Dfc.CourseDirectory.Web.ViewModels.ProviderSearch
{
    public class ProviderSearchViewModel
    {
        [RegularExpression(@"[a-zA-Z0-9 \¬\!\£\$\%\^\&\*\(\)_\+\-\=\{\}\[\]\;\:\@\'\#\~\,\<\>\.\?\/\|\`\" + "\"" + @"\\]+", ErrorMessage = "Search contains invalid characters")]
        public string Search { get; set; }
        public IEnumerable<ProviderAzureSearchResultItem> Providers { get; set;}


        //[RegularExpression(@"[a-zA-Z0-9 \¬\!\£\$\%\^\&\*\(\)_\+\-\=\{\}\[\]\;\:\@\'\#\~\,\<\>\.\?\/\|\`\" + "\"" + @"\\]+", ErrorMessage = "Search contains invalid characters")]
        //public string Search { get; set; }
        //public bool HasFilters { get; set; }
        //public IList<ProviderCourseRunViewModel> ProviderCourseRuns { get; set;}
        //public List<ProviderCoursesFilterItemModel> Levels { get; set; }
        //public List<ProviderCoursesFilterItemModel> DeliveryModes { get; set; }
        //public List<ProviderCoursesFilterItemModel> Venues { get; set; }
        //public List<ProviderCoursesFilterItemModel> Regions { get; set; }
        //public List<ProviderCoursesFilterItemModel> AttendancePattern { get; set; }
        //public int? PendingCoursesCount { get; set; }
        //public string NotificationTitle { get; set; }
        //public string NotificationMessage { get; set; }
        //public string CourseRunId { get; set; }
    }
}
