using System;

namespace Dfc.CourseDirectory.WebV2.Behaviors
{
    public interface IRestrictQAStatus<in TRequest>
    {
        Guid GetProviderId(TRequest request);
    }
}
