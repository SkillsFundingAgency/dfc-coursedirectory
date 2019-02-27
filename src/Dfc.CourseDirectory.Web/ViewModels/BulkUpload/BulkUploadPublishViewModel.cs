using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;

namespace Dfc.CourseDirectory.Web.ViewModels.BulkUpload
{
    public class BulkUploadPublishViewModel
    {
       public Guid? CourseId { get; set; }

        public IEnumerable<Course> Courses { get; set; }
    }
}
