using System;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.WebV2.Security;
using MediatR;

namespace Dfc.CourseDirectory.WebV2.Behaviors
{
    public class RequireUserCanAccessCourseBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly IRequireUserCanAccessCourse<TRequest> _descriptor;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IProviderOwnershipCache _providerOwnershipCache;
        private readonly IProviderContextProvider _providerContextProvider;
        private readonly IProviderInfoCache _providerInfoCache;

        public RequireUserCanAccessCourseBehavior(
            IRequireUserCanAccessCourse<TRequest> descriptor,
            ICurrentUserProvider currentUserProvider,
            IProviderOwnershipCache providerOwnershipCache,
            IProviderContextProvider providerContextProvider,
            IProviderInfoCache providerInfoCache)
        {
            _descriptor = descriptor;
            _currentUserProvider = currentUserProvider;
            _providerOwnershipCache = providerOwnershipCache;
            _providerContextProvider = providerContextProvider;
            _providerInfoCache = providerInfoCache;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            CancellationToken cancellationToken,
            RequestHandlerDelegate<TResponse> next)
        {
            var courseId = _descriptor.GetCourseId(request);
            var providerId = await _providerOwnershipCache.GetProviderForCourse(courseId);

            if (providerId == null)
            {
                throw new ResourceDoesNotExistException(ResourceType.Course, courseId);
            }

            if (IsAuthorized(providerId))
            {
                var providerInfo = await _providerInfoCache.GetProviderInfo(providerId.Value);
                _providerContextProvider.SetProviderContext(new ProviderContext(providerInfo));

                return await next();
            }
            else
            {
                throw new NotAuthorizedException();
            }
        }

        private bool IsAuthorized(Guid? providerId)
        {
            var currentUser = _currentUserProvider.GetCurrentUser();

            if (currentUser == null)
            {
                return false;
            }

            if (currentUser.Role == RoleNames.Developer || currentUser.Role == RoleNames.Helpdesk)
            {
                return true;
            }

            if ((currentUser.Role == RoleNames.ProviderUser || currentUser.Role == RoleNames.ProviderSuperUser) &&
                currentUser.CurrentProviderId == providerId)
            {
                return true;
            }

            return false;
        }
    }
}
