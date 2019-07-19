using Dfc.CourseDirectory.Models.Models.Courses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Web.ViewModels.BulkUpload
{
    public class BulkUploadViewModel
    {
        public IEnumerable<string> errors { get; set; }
        public string NotificationTitle { get; set; }
        public string NotificationMessage { get; set; }
        public bool HasMigrationErrors { get; set; }
    }
}
