using System;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.WebV2.Behaviors
{
    public interface IRequireUserCanSubmitQASubmission<in TRequest>
    {
        Task<Guid> GetProviderId(TRequest request);
    }
}
