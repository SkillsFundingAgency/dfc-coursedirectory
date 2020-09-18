using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.WebV2.Security;
using MediatR;

namespace Dfc.CourseDirectory.WebV2.Behaviors
{
    public class RequireUserCanAccessVenueBehavior<TRequest, TResponse>
        : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly IRequireUserCanAccessVenue<TRequest> _descriptor;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IProviderOwnershipCache _providerOwnershipCache;
        private readonly IProviderContextProvider _providerContextProvider;
        private readonly IProviderInfoCache _providerInfoCache;

        public RequireUserCanAccessVenueBehavior(
            IRequireUserCanAccessVenue<TRequest> descriptor,
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

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            var VenueId = _descriptor.GetVenueId(request);
            var providerId = await _providerOwnershipCache.GetProviderForVenue(VenueId);

            if (providerId == null)
            {
                throw new ResourceDoesNotExistException(ResourceType.Venue, VenueId);
            }

            var isAuthorized = IsAuthorized();

            if (isAuthorized)
            {
                var providerInfo = await _providerInfoCache.GetProviderInfo(providerId.Value);
                _providerContextProvider.SetProviderContext(new ProviderContext(providerInfo));

                return await next();
            }
            else
            {
                throw new NotAuthorizedException();
            }

            bool IsAuthorized()
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
}
