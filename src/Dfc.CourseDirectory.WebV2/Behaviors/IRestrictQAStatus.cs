using System.Collections.Generic;
using Dfc.CourseDirectory.WebV2.Models;

namespace Dfc.CourseDirectory.WebV2.Behaviors
{
    public interface IRestrictQAStatus<in TRequest>
        where TRequest : IProviderScopedRequest
    {
        IEnumerable<ApprenticeshipQAStatus> PermittedStatuses { get; }
    }
}
