using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.Providerportal.FindAnApprenticeship.Interfaces.Models
{
    public interface IFeChoice
    {
        Guid Id { get; }
        int UKPRN { get; }
        double? LearnerSatisfaction { get; }
        double? EmployerSatisfaction { get; }
        DateTime? CreatedDateTimeUtc { get; }
    }
}
