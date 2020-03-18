using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Models;

namespace Dfc.CourseDirectory.WebV2.Behaviors
{
    public interface IRestrictProviderType<in TRequest>
    {
        ProviderType ProviderType { get; }
        Task<Guid> GetProviderId(TRequest request);
    }
}
