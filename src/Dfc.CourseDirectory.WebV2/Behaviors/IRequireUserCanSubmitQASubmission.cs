namespace Dfc.CourseDirectory.WebV2.Behaviors
{
    public interface IRequireUserCanSubmitQASubmission<in TRequest>
        where TRequest : IProviderScopedRequest
    {
    }
}
