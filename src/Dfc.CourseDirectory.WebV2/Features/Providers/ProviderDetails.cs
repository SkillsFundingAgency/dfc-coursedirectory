using System;
using System.Collections.Generic;
using System.Linq;
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
using SqlModels = Dfc.CourseDirectory.Core.DataStore.Sql.Models;
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
        public IEnumerable<TLevelDefinitionViewModel> ProviderTLevelDefinitions { get; set; }
    }

    public class Handler : IRequestHandler<Query, ViewModel>
    {
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly ICurrentUserProvider _currentUserProvider;

        public Handler(
            ISqlQueryDispatcher sqlQueryDispatcher,
            ICurrentUserProvider currentUserProvider)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _currentUserProvider = currentUserProvider;
        }

        public async Task<ViewModel> Handle(Query request, CancellationToken cancellationToken)
        {
           

            var sqlProvider = await _sqlQueryDispatcher.ExecuteQuery(new SqlQueries.GetProviderById()
            {
                ProviderId = request.ProviderId
            });

           

            var currentUser = _currentUserProvider.GetCurrentUser();

            var providerTLevelDefinitions = sqlProvider.ProviderType.HasFlag(ProviderType.TLevels)
                ? await _sqlQueryDispatcher.ExecuteQuery(new SqlQueries.GetTLevelDefinitionsForProvider { ProviderId = request.ProviderId })
                : Enumerable.Empty<SqlModels.TLevelDefinition>();

            return new ViewModel()
            {
                ProviderId = request.ProviderId,
                ProviderName = sqlProvider.ProviderName,
                CourseDirectoryStatus = sqlProvider.ProviderStatus,
                Ukprn = sqlProvider.Ukprn,
                TradingName = sqlProvider.Alias,
                DisplayName = sqlProvider.DisplayName,
                CanChangeDisplayName = sqlProvider.HaveAlias && AuthorizationRules.CanUpdateProviderDisplayName(currentUser),
                ProviderType = sqlProvider.ProviderType,
                CanChangeProviderType = AuthorizationRules.CanUpdateProviderType(currentUser),
                MarketingInformation = sqlProvider.MarketingInformation != null ?
                    Html.SanitizeHtml(sqlProvider.MarketingInformation) :
                    null,
                CanUpdateMarketingInformation = AuthorizationRules.CanUpdateProviderMarketingInformation(currentUser),
                ProviderTLevelDefinitions = providerTLevelDefinitions.Select(d => new TLevelDefinitionViewModel
                {
                    TLevelDefinitionId = d.TLevelDefinitionId,
                    Name = d.Name
                })
            };
        }
    }
}
