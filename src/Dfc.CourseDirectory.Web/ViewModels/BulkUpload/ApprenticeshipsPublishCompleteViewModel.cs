using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Services.Models;

namespace Dfc.CourseDirectory.Web.ViewModels.BulkUpload
{
    public class ApprenticeshipsPublishCompleteViewModel
    {
        
        public int NumberOfApprenticeshipsPublished { get; set; }

        public PublishMode Mode { get; set; }

        public bool BackgroundPublishInProgress { get; set; }
    }
}
