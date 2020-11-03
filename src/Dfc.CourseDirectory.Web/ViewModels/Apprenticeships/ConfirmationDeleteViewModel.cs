using System;
using Dfc.CourseDirectory.Services.Enums;

namespace Dfc.CourseDirectory.Web.ViewModels.Apprenticeships
{
    public class ConfirmationDeleteViewModel
    {
        public string ApprenticeshipTitle { get; set; }
        public Guid ApprenticeshipId { get; set; }
        public int Level { get; set; }
        public ApprenticeshipDelete ApprenticeshipDelete { get; set; }
    }
}
