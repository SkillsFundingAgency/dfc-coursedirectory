using System;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Validation;
using Dfc.CourseDirectory.WebV2.Security;
using MediatR;

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
        public ProviderType ProviderType { get; set; }
        public bool CanChangeProviderType { get; set; }
        public string MarketingInformation { get; set; }
        public bool ShowMarketingInformation { get; set; }
        public bool CanUpdateMarketingInformation { get; set; }
    }

    public class Handler : IRequestHandler<Query, ViewModel>
    {
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly ICurrentUserProvider _currentUserProvider;

        public Handler(
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher,
            ICurrentUserProvider currentUserProvider)
        {
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
            _currentUserProvider = currentUserProvider;
        }

        public async Task<ViewModel> Handle(Query request, CancellationToken cancellationToken)
        {
            var provider = await _cosmosDbQueryDispatcher.ExecuteQuery(new GetProviderById()
            {
                ProviderId = request.ProviderId
            });

            if (provider == null)
            {
                throw new ResourceDoesNotExistException(ResourceType.Provider, request.ProviderId);
            }

            var currentUser = _currentUserProvider.GetCurrentUser();

            return new ViewModel()
            {
                ProviderId = request.ProviderId,
                ProviderName = provider.ProviderName,
                CourseDirectoryStatus = provider.ProviderStatus,
                Ukprn = provider.Ukprn,
                TradingName = provider.Alias,
                ProviderType = provider.ProviderType,
                CanChangeProviderType = AuthorizationRules.CanUpdateProviderType(currentUser),
                MarketingInformation = provider.MarketingInformation != null ?
                    Html.SanitizeHtml(provider.MarketingInformation) :
                    null,
                ShowMarketingInformation = provider.ProviderType.HasFlag(ProviderType.Apprenticeships),
                CanUpdateMarketingInformation = AuthorizationRules.CanUpdateProviderMarketingInformation(currentUser)
            };
        }
    }
}
