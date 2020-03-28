using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.WebV2.Validation;

namespace Dfc.CourseDirectory.WebV2.Features.NewApprenticeshipProvider
{
    public class FlowModelInitializer
    {
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;

        public FlowModelInitializer(ICosmosDbQueryDispatcher cosmosDbQueryDispatcher)
        {
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
        }

        public async Task<FlowModel> Initialize(Guid providerId)
        {
            var provider = await _cosmosDbQueryDispatcher.ExecuteQuery(
                new GetProviderById()
                {
                    ProviderId = providerId
                });

            var model = new FlowModel();

            if (!string.IsNullOrEmpty(provider.MarketingInformation))
            {
                model.SetProviderDetails(Html.SanitizeHtml(provider.MarketingInformation));
            }

            return model;
        }
    }
}
