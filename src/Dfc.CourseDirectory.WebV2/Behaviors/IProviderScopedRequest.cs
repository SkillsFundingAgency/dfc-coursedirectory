using System;

namespace Dfc.CourseDirectory.WebV2.Behaviors
{
    public interface IProviderScopedRequest
    {
        Guid ProviderId { get; }
    }
}
