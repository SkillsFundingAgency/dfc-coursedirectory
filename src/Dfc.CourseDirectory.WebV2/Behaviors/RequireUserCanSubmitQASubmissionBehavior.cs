using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.WebV2.Security;
using MediatR;

namespace Dfc.CourseDirectory.WebV2.Behaviors
{
    public class RequireUserCanSubmitQASubmissionBehavior<TRequest, TResponse>
        : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly IRequireUserCanSubmitQASubmission<TRequest> _descriptor;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly IProviderInfoCache _providerInfoCache;

        public RequireUserCanSubmitQASubmissionBehavior(
            IRequireUserCanSubmitQASubmission<TRequest> descriptor,
            ICurrentUserProvider currentUserProvider,
            ISqlQueryDispatcher sqlQueryDispatcher,
            IProviderInfoCache providerInfoCache)
        {
            _descriptor = descriptor;
            _currentUserProvider = currentUserProvider;
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _providerInfoCache = providerInfoCache;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            CancellationToken cancellationToken,
            RequestHandlerDelegate<TResponse> next)
        {
            var providerId = _descriptor.GetProviderId(request);
            var currentUser = _currentUserProvider.GetCurrentUser();

            if (!AuthorizationRules.CanSubmitQASubmission(currentUser, providerId))
            {
                throw new NotAuthorizedException();
            }

            var qaStatus = await _sqlQueryDispatcher.ExecuteQuery(
                new GetProviderApprenticeshipQAStatus()
                {
                    ProviderId = providerId
                });

            var effectiveQaStatus = qaStatus.ValueOrDefault();

            // Ignore UnableToComplete here
            var qaStatusIsValid = (effectiveQaStatus & ~ApprenticeshipQAStatus.UnableToComplete) switch
            {
                ApprenticeshipQAStatus.NotStarted => true,
                ApprenticeshipQAStatus.Failed => true,
                _ => false
            };

            var providerInfo = await _providerInfoCache.GetProviderInfo(providerId);
            var providerTypeIsValid = providerInfo.ProviderType.HasFlag(ProviderType.Apprenticeships);

            if (!qaStatusIsValid || !providerTypeIsValid)
            {
                throw new InvalidStateException(InvalidStateReason.InvalidApprenticeshipQAStatus);
            }

            return await next();
        }
    }
}
