using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Models;

namespace Dfc.CourseDirectory.WebV2.Behaviors
{
    public interface IRestrictQAStatus<in TRequest>
    {
        Task<Guid> GetProviderId(TRequest request);
        IEnumerable<ApprenticeshipQAStatus> PermittedStatuses { get; }
    }
}
