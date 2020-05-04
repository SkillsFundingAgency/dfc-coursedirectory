﻿using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dfc.CourseDirectory.WebV2.SharedViews.Components
{
    public class AdminProviderContextNavViewComponent : ViewComponent
    {
        private readonly IFeatureFlagProvider _featureFlagProvider;
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;

        public AdminProviderContextNavViewComponent(
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
                ApprenticeshipQAStatus = qaStatus ?? ApprenticeshipQAStatus.NotStarted,
                ProviderContext = providerInfo
            };

            return View("~/SharedViews/Components/AdminProviderContextNav.cshtml", vm);
        }
    }
}
