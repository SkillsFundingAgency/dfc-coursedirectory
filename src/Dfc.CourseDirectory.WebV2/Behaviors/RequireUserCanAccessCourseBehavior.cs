using System;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.WebV2.Security;
using MediatR;

namespace Dfc.CourseDirectory.WebV2.Behaviors
{
    public class RequireUserCanAccessCourseBehavior<TRequest, TResponse>
        : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly IRequireUserCanAccessCourse<TRequest> _descriptor;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IProviderOwnershipCache _providerOwnershipCache;

        public RequireUserCanAccessCourseBehavior(
            IRequireUserCanAccessCourse<TRequest> descriptor,
            ICurrentUserProvider currentUserProvider,
            IProviderOwnershipCache providerOwnershipCache)
        {
            _descriptor = descriptor;
            _currentUserProvider = currentUserProvider;
            _providerOwnershipCache = providerOwnershipCache;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            var courseId = _descriptor.GetCourseId(request);
            var providerId = await _providerOwnershipCache.GetProviderForCourse(courseId);

            if (providerId == null)
            {
                throw new ResourceDoesNotExistException(ResourceType.Course, courseId);
            }

            var currentUser = _currentUserProvider.GetCurrentUser();

            if (currentUser == null)
            {
                throw new NotAuthorizedException();
            }

            if (currentUser.Role == RoleNames.Developer || currentUser.Role == RoleNames.Helpdesk)
            {
                return await next();
            }

            if ((currentUser.Role == RoleNames.ProviderUser || currentUser.Role == RoleNames.ProviderSuperUser) &&
                currentUser.CurrentProviderId == providerId)
            {
                return await next();
            }

            throw new NotAuthorizedException();
        }
    }
}
