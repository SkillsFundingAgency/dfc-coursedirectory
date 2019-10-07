using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Models.Enums;
using Dfc.CourseDirectory.Models.Interfaces.Apprenticeships;
using Dfc.CourseDirectory.Models.Models.Apprenticeships;
using Dfc.CourseDirectory.Models.Models.Regions;

namespace Dfc.CourseDirectory.Web.ViewModels.PublishApprenticeships
{
    public class PublishApprenticeshipsViewModel
    {
        public IEnumerable<IApprenticeship> ListOfApprenticeships { get; set; }
        public int NumberOfApprenticeships { get; set; }
        public bool AreAllReadyToBePublished { get; set; }
        public string NotificationTitle { get; set; }
        public string NotificationMessage { get; set; }

    }
}
