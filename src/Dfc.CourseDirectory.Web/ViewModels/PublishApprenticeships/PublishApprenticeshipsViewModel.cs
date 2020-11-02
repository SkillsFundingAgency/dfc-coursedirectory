using System.Collections.Generic;
using Dfc.CourseDirectory.Models.Models.Apprenticeships;

namespace Dfc.CourseDirectory.Web.ViewModels.PublishApprenticeships
{
    public class PublishApprenticeshipsViewModel
    {
        public IEnumerable<Apprenticeship> ListOfApprenticeships { get; set; }
        public int NumberOfApprenticeships { get; set; }
        public bool AreAllReadyToBePublished { get; set; }
        public string NotificationTitle { get; set; }
        public string NotificationMessage { get; set; }
    }
}
