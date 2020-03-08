using Dfc.CourseDirectory.WebV2.Security;
using MediatR;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Features.HelpdeskDashboard.Dashboard
{
    public enum ErrorReason
    {
        NotAuthorized,
        NothingAvailable
    }

    public class Query : IRequest<OneOf<Error<ErrorReason>, ViewModel>>
    {
    }

    public class ViewModel
    {
        public bool CanAccessApprenticeshipQA { get; set; }
    }

    public class QueryHandler : RequestHandler<Query, OneOf<Error<ErrorReason>, ViewModel>>
    {
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IFeatureFlagProvider _featureFlagProvider;

        public QueryHandler(
            ICurrentUserProvider currentUserProvider,
            IFeatureFlagProvider featureFlagProvider)
        {
            _currentUserProvider = currentUserProvider;
            _featureFlagProvider = featureFlagProvider;
        }

        protected override OneOf<Error<ErrorReason>, ViewModel> Handle(Query request)
        {
            var currentUser = _currentUserProvider.GetCurrentUser();

            if (currentUser.Role != RoleNames.Developer && currentUser.Role != RoleNames.Helpdesk)
            {
                return new Error<ErrorReason>(ErrorReason.NotAuthorized);
            }

            var canAccessApprenticeshipQA = _featureFlagProvider.HaveFeature(FeatureFlags.ApprenticeshipQA);

            if (!canAccessApprenticeshipQA)
            {
                return new Error<ErrorReason>(ErrorReason.NothingAvailable);
            }

            return new ViewModel()
            {
                CanAccessApprenticeshipQA = canAccessApprenticeshipQA
            };
        }
    }
}
