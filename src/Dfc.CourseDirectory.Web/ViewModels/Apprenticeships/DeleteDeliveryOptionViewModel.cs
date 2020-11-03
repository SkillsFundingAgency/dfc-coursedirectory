using System;

namespace Dfc.CourseDirectory.Web.ViewModels.Apprenticeships
{
    public class DeleteDeliveryOptionViewModel
    {
        public string LocationName { get; set; }
        public string ApprenticeshipTitle { get; set; }
        public Guid Id { get; set; }
        public bool Combined { get; set; }
    }
}
