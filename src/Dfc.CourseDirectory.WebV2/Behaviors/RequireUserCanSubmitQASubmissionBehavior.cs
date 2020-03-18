using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Security;
using MediatR;

namespace Dfc.CourseDirectory.WebV2.Behaviors
{
    public class RequireUserCanSubmitQASubmissionBehavior<TRequest, TResponse>
        : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly IRequireUserCanSubmitQASubmission<TRequest> _descriptor;
        private readonly ICurrentUserProvider _currentUserProvider;

        public RequireUserCanSubmitQASubmissionBehavior(
            IRequireUserCanSubmitQASubmission<TRequest> descriptor,
            ICurrentUserProvider currentUserProvider)
        {
            _descriptor = descriptor;
            _currentUserProvider = currentUserProvider;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            CancellationToken cancellationToken,
            RequestHandlerDelegate<TResponse> next)
        {
            var providerId = await _descriptor.GetProviderId(request);
            var currentUser = _currentUserProvider.GetCurrentUser();

            if (!AuthorizationRules.CanSubmitQASubmission(currentUser, providerId))
            {
                throw new NotAuthorizedException();
            }

            return await next();
        }
    }
}
