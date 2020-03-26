using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.DataStore.Sql;
using Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.SharedViews.Components
{
    public class ProviderTopNavViewComponent : ViewComponent
    {
        private readonly IFeatureFlagProvider _featureFlagProvider;
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;

        public ProviderTopNavViewComponent(
            IFeatureFlagProvider featureFlagProvider,
            ISqlQueryDispatcher sqlQueryDispatcher)
        {
            _featureFlagProvider = featureFlagProvider;
            _sqlQueryDispatcher = sqlQueryDispatcher;
        }

        public async Task<IViewComponentResult> InvokeAsync(ProviderInfo providerInfo)
        {
            var qaStatus = await _sqlQueryDispatcher.ExecuteQuery(
                new GetProviderApprenticeshipQAStatus()
                {
                    ProviderId = providerInfo.ProviderId
                });

            var vm = new ProviderNavViewModel()
            {
                ApprenticeshipQAFeatureIsEnabled = _featureFlagProvider.HaveFeature(FeatureFlags.ApprenticeshipQA),
                ApprenticeshipQAStatus = qaStatus ?? Models.ApprenticeshipQAStatus.NotStarted,
                ProviderContext = providerInfo
            };

            return View("~/SharedViews/Components/ProviderTopNav.cshtml", vm);
        }
    }
}
