using System;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.WebV2.Behaviors
{
    public interface IRequireUserCanSubmitQASubmission<in TRequest, TResponse>
    {
        Task<Guid> GetProviderId(TRequest request);
    }
}
