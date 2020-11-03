using Dfc.CourseDirectory.Services.Enums;

namespace Dfc.CourseDirectory.Web.ViewModels.BulkUpload
{
    public class WhatDoYouWantToDoNextViewModel
    {
        public WhatDoYouWantToDoNext WhatDoYouWantToDoNext { get; set; }
        public string Message { get; set; }
        public int ErrorCount { get; set; }
    }
}
