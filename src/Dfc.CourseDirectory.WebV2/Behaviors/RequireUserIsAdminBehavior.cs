using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Security;
using MediatR;

namespace Dfc.CourseDirectory.WebV2.Behaviors
{
    public class RequireUserIsAdminBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly ICurrentUserProvider _currentUserProvider;

        public RequireUserIsAdminBehavior(ICurrentUserProvider currentUserProvider)
        {
            _currentUserProvider = currentUserProvider;
        }
        
        public Task<TResponse> Handle(
            TRequest request,
            CancellationToken cancellationToken,
            RequestHandlerDelegate<TResponse> next)
        {
            var currentUser = _currentUserProvider.GetCurrentUser();

            if (currentUser != null && (currentUser.IsDeveloper || currentUser.IsHelpdesk))
            {
                return next();
            }

            throw new NotAuthorizedException();
        }
    }
}
