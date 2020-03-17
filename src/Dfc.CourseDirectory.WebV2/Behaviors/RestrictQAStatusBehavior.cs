using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.DataStore.Sql;
using Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries;
using Dfc.CourseDirectory.WebV2.Models;
using MediatR;

namespace Dfc.CourseDirectory.WebV2.Behaviors
{
    public class RestrictQAStatusBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly IRestrictQAStatus<TRequest, TResponse> _descriptor;
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;

        public RestrictQAStatusBehavior(
            IRestrictQAStatus<TRequest, TResponse> descriptor,
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
            var providerId = await _descriptor.GetProviderId(request);

            var qaStatus = await _sqlQueryDispatcher.ExecuteQuery(
                new GetProviderApprenticeshipQAStatus()
                {
                    ProviderId = providerId
                });

            var isPermitted = qaStatus.HasFlag(ApprenticeshipQAStatus.UnableToComplete) ?
                _descriptor.PermittedStatuses.Contains(ApprenticeshipQAStatus.UnableToComplete) :
                _descriptor.PermittedStatuses.Contains(qaStatus);

            if (!isPermitted)
            {
                return _descriptor.CreateErrorResponse();
            }

            return await next();
        }
    }
}
