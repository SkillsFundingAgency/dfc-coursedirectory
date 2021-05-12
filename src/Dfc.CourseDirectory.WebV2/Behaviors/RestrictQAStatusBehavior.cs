using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using MediatR;

namespace Dfc.CourseDirectory.WebV2.Behaviors
{
    public class RestrictQAStatusBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly IRestrictQAStatus<TRequest> _descriptor;
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;

        public RestrictQAStatusBehavior(
            IRestrictQAStatus<TRequest> descriptor,
            ISqlQueryDispatcher sqlQueryDispatcher)
        {
            _descriptor = descriptor;
            _sqlQueryDispatcher = sqlQueryDispatcher;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            CancellationToken cancellationToken,
            RequestHandlerDelegate<TResponse> next)
        {
            var providerId = _descriptor.GetProviderId(request);

            var qaStatus = await _sqlQueryDispatcher.ExecuteQuery(
                new GetProviderApprenticeshipQAStatus()
                {
                    ProviderId = providerId
                });

            var effectiveQaStatus = qaStatus.ValueOrDefault();

            var isPermitted = effectiveQaStatus.HasFlag(ApprenticeshipQAStatus.UnableToComplete) ?
                _descriptor.PermittedStatuses.Contains(ApprenticeshipQAStatus.UnableToComplete) :
                _descriptor.PermittedStatuses.Contains(effectiveQaStatus);

            if (!isPermitted)
            {
                throw new InvalidStateException(InvalidStateReason.InvalidApprenticeshipQAStatus);
            }

            return await next();
        }
    }
}
