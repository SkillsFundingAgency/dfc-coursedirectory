using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.Features.NewApprenticeshipProvider
{
    public class QANotificationsViewComponent : ViewComponent
    {
        private readonly IProviderContextProvider _providerContextProvider;
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly IProviderInfoCache _providerInfoCache;

        public QANotificationsViewComponent(
            IProviderContextProvider providerContextProvider,
            ISqlQueryDispatcher sqlQueryDispatcher,
            IProviderInfoCache providerInfoCache)
        {
            _providerContextProvider = providerContextProvider;
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _providerInfoCache = providerInfoCache;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var providerId = _providerContextProvider.GetProviderContext().ProviderId;

            var qaStatus = await _sqlQueryDispatcher.ExecuteQuery(
                new GetProviderApprenticeshipQAStatus()
                {
                    ProviderId = providerId
                });

            var submission = await _sqlQueryDispatcher.ExecuteQuery(
                new GetLatestApprenticeshipQASubmissionForProvider()
                {
                    ProviderId = providerId
                });

            var providerInfo = await _providerInfoCache.GetProviderInfo(providerId);

            var vm = new QANotificationsViewModel()
            {
                ProviderType = providerInfo.ProviderType,
                Status = qaStatus.ValueOrDefault(),
                HidePassedNotication = submission.Match(none => false, sub => sub.HidePassedNotification)
            };

            return View("~/Features/NewApprenticeshipProvider/QANotifications.cshtml", vm);
        }
    }
}
