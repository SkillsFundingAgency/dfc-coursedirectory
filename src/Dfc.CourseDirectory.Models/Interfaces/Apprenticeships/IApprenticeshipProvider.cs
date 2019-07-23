using Dfc.CourseDirectory.Models.Models.Apprenticeships;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Models.Interfaces.Apprenticeships
{
    public interface IApprenticeshipProvider
    {
        Guid Id { get; set; }
        int TribalId { get; set; }
        string Email { get; set; }
        double? EmployerSatisfaction { get; set; }
        List<Framework> Frameworks { get; set; }
        double? LearnerSatisfaction { get; set; }
        List<ApprenticeshipLocation> Locations { get; set; }
        string MarketingInfo { get; set; }
        string Name { get; set; }
        bool NationalProvider { get; set; }
        string Phone { get; set; }
        List<Standard> Standards { get; set; }
        int UKPRN { get; set; }
        string Website { get; set; }
    }
}
