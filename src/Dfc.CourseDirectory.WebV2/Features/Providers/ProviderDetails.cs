using System;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Validation;
using Dfc.CourseDirectory.WebV2.Security;
using MediatR;
using CosmosQueries = Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using SqlQueries = Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.WebV2.Features.Providers.ProviderDetails
{
    public class Query : IRequest<ViewModel>
    {
        public Guid ProviderId { get; set; }
    }

    public class ViewModel
    {
        public Guid ProviderId { get; set; }
        public string ProviderName { get; set; }
        public string CourseDirectoryStatus { get; set; }
        public int Ukprn { get; set; }
        public string TradingName { get; set; }
        public string DisplayName { get; set; }
        public bool CanChangeDisplayName { get; set; }
        public ProviderType ProviderType { get; set; }
        public bool CanChangeProviderType { get; set; }
        public string MarketingInformation { get; set; }
        public bool ShowMarketingInformation { get; set; }
        public bool CanUpdateMarketingInformation { get; set; }
    }

    public class Handler : IRequestHandler<Query, ViewModel>
    {
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly ICurrentUserProvider _currentUserProvider;

        public Handler(
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher,
            ISqlQueryDispatcher sqlQueryDispatcher,
            ICurrentUserProvider currentUserProvider)
        {
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _currentUserProvider = currentUserProvider;
        }

        public async Task<ViewModel> Handle(Query request, CancellationToken cancellationToken)
        {
            var cosmosProvider = await _cosmosDbQueryDispatcher.ExecuteQuery(new CosmosQueries.GetProviderById()
            {
                ProviderId = request.ProviderId
            });

            var sqlProvider = await _sqlQueryDispatcher.ExecuteQuery(new SqlQueries.GetProviderById()
            {
                ProviderId = request.ProviderId
            });

            if (cosmosProvider == null)
            {
                throw new ResourceDoesNotExistException(ResourceType.Provider, request.ProviderId);
            }

            var currentUser = _currentUserProvider.GetCurrentUser();

            return new ViewModel()
            {
                ProviderId = request.ProviderId,
                ProviderName = cosmosProvider.ProviderName,
                CourseDirectoryStatus = cosmosProvider.ProviderStatus,
                Ukprn = cosmosProvider.Ukprn,
                TradingName = cosmosProvider.Alias,
                DisplayName = sqlProvider.DisplayName,
                CanChangeDisplayName = sqlProvider.HaveAlias,
                ProviderType = cosmosProvider.ProviderType,
                CanChangeProviderType = AuthorizationRules.CanUpdateProviderType(currentUser),
                MarketingInformation = cosmosProvider.MarketingInformation != null ?
                    Html.SanitizeHtml(cosmosProvider.MarketingInformation) :
                    null,
                ShowMarketingInformation = cosmosProvider.ProviderType.HasFlag(ProviderType.Apprenticeships),
                CanUpdateMarketingInformation = AuthorizationRules.CanUpdateProviderMarketingInformation(currentUser)
            };
        }
    }
}
