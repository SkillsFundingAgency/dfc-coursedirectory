using System;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.WebV2.Security;
using MediatR;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Features.Providers.DisplayName
{
    public class Query : IRequest<ViewModel>
    {
        public Guid ProviderId { get; set; }
    }

    public class Command : IRequest<Success>
    {
        public Guid ProviderId { get; set; }
        public ProviderDisplayNameSource DisplayNameSource { get; set; }
    }

    public class ViewModel : Command
    {
        public string ProviderName { get; set; }
        public string TradingName { get; set; }
    }

    public class Handler :
        IRequestHandler<Query, ViewModel>,
        IRequestHandler<Command, Success>
    {
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly ICurrentUserProvider _currentUserProvider;

        public Handler(ISqlQueryDispatcher sqlQueryDispatcher, ICurrentUserProvider currentUserProvider)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _currentUserProvider = currentUserProvider;
        }

        public async Task<ViewModel> Handle(Query request, CancellationToken cancellationToken)
        {
            CheckUserPermissions();

            var provider = await GetProvider(request.ProviderId);
            CheckHaveAlias(provider);

            return new ViewModel()
            {
                ProviderId = request.ProviderId,
                ProviderName = provider.ProviderName,
                TradingName = provider.Alias,
                DisplayNameSource = provider.DisplayNameSource
            };
        }

        public async Task<Success> Handle(Command request, CancellationToken cancellationToken)
        {
            CheckUserPermissions();

            var provider = await GetProvider(request.ProviderId);
            CheckHaveAlias(provider);

            var result = await _sqlQueryDispatcher.ExecuteQuery(new SetProviderDisplayNameSource()
            {
                ProviderId = request.ProviderId,
                DisplayNameSource = request.DisplayNameSource
            });

            if (result.Value is NotFound)
            {
                throw new ResourceDoesNotExistException(ResourceType.Provider, request.ProviderId);
            }

            return new Success();
        }

        private void CheckHaveAlias(Provider provider)
        {
            if (!provider.HaveAlias)
            {
                throw new InvalidStateException();
            }
        }

        private void CheckUserPermissions()
        {
            var currentUser = _currentUserProvider.GetCurrentUser();

            if (currentUser.Role == RoleNames.ProviderUser)
            {
                throw new NotAuthorizedException();
            }
        }

        private async Task<Provider> GetProvider(Guid providerId)
        {
            var provider = await _sqlQueryDispatcher.ExecuteQuery(new GetProviderById()
            {
                ProviderId = providerId
            });

            if (provider == null)
            {
                throw new ResourceDoesNotExistException(ResourceType.Provider, providerId);
            }

            return provider;
        }
    }
}
