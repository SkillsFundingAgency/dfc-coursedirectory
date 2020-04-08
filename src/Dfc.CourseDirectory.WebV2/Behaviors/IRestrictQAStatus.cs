using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.WebV2.Models;

namespace Dfc.CourseDirectory.WebV2.Behaviors
{
    public interface IRestrictQAStatus<in TRequest>
    {
        Guid GetProviderId(TRequest request);
        IEnumerable<ApprenticeshipQAStatus> PermittedStatuses { get; }
    }
}
