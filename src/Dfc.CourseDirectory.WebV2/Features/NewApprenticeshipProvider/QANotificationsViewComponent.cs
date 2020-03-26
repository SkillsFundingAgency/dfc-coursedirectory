using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.DataStore.Sql;
using Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.NewApprenticeshipProvider
{
    public class QANotificationsViewComponent : ViewComponent
    {
        private readonly IProviderContextProvider _providerContextProvider;
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;

        public QANotificationsViewComponent(
            IProviderContextProvider providerContextProvider,
            ISqlQueryDispatcher sqlQueryDispatcher)
        {
            _providerContextProvider = providerContextProvider;
            _sqlQueryDispatcher = sqlQueryDispatcher;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var providerId = _providerContextProvider.GetProviderContext().ProviderId;

            var qaStatus = await _sqlQueryDispatcher.ExecuteQuery(
                new GetProviderApprenticeshipQAStatus()
                {
                    ProviderId = providerId
                }) ?? Models.ApprenticeshipQAStatus.NotStarted;

            return View("~/Features/NewApprenticeshipProvider/QANotifications.cshtml", qaStatus);
        }
    }
}
