using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.WebV2.HttpContextFeatures;
using Dfc.CourseDirectory.WebV2.MultiPageTransaction;
using Dfc.CourseDirectory.WebV2.Validation;
using Microsoft.AspNetCore.Http;

namespace Dfc.CourseDirectory.WebV2.Features.NewApprenticeshipProvider
{
    public class FlowModelInitializer : IInitializeMptxState<FlowModel>
    {
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FlowModelInitializer(
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher,
            IHttpContextAccessor httpContextAccessor)
        {
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task Initialize(MptxInstanceContext<FlowModel> context)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var providerId = httpContext.Features.Get<ProviderContextFeature>().ProviderInfo.ProviderId;

            var provider = await _cosmosDbQueryDispatcher.ExecuteQuery(
                new GetProviderById()
                {
                    ProviderId = providerId
                });

            if (!string.IsNullOrEmpty(provider.MarketingInformation))
            {
                context.Update(s => s.SetProviderDetail(Html.SanitizeHtml(provider.MarketingInformation)));
            }
        }
    }
}
