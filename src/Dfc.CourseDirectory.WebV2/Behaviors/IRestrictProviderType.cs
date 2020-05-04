using System;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.WebV2.Behaviors
{
    public interface IRestrictProviderType<in TRequest>
    {
        ProviderType ProviderType { get; }
        Guid GetProviderId(TRequest request);
    }
}
