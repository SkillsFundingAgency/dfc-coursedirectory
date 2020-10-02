using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.SharedViews.Components
{
    public class ProviderTopNavViewComponent : ViewComponent
    {
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;

        public ProviderTopNavViewComponent(ISqlQueryDispatcher sqlQueryDispatcher)
        {
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
                ApprenticeshipQAStatus = qaStatus ?? ApprenticeshipQAStatus.NotStarted,
                ProviderContext = providerInfo,
                ShowApprenticeshipsLink = providerInfo.ProviderType.HasFlag(ProviderType.Apprenticeships) &&
                    qaStatus == ApprenticeshipQAStatus.Passed
            };

            return View("~/SharedViews/Components/ProviderTopNav.cshtml", vm);
        }
    }
}
