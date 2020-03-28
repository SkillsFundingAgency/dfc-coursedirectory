using Dfc.CourseDirectory.WebV2.Models;

namespace Dfc.CourseDirectory.WebV2.Behaviors
{
    public interface IRestrictProviderType<in TRequest>
        where TRequest : IProviderScopedRequest
    {
        ProviderType ProviderType { get; }
    }
}
