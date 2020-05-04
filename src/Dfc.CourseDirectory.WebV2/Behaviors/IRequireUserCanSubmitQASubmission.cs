using System;

namespace Dfc.CourseDirectory.WebV2.Behaviors
{
    public interface IRequireUserCanSubmitQASubmission<in TRequest>
    {
        Guid GetProviderId(TRequest request);
    }
}
