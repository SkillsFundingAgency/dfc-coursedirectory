using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2;
using Dfc.CourseDirectory.WebV2.DataStore.Sql;
using Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries;
using Dfc.CourseDirectory.WebV2.HttpContextFeatures;
using Dfc.CourseDirectory.WebV2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.Web
{
    public class RestrictApprenticeshipQAStatusAttribute : ActionFilterAttribute
    {
        public RestrictApprenticeshipQAStatusAttribute(params ApprenticeshipQAStatus[] allowedStatuses)
        {
            AllowedStatuses = new HashSet<ApprenticeshipQAStatus>(allowedStatuses);
            AllowWhenApprenticeshipQAFeatureDisabled = true;
        }

        public ISet<ApprenticeshipQAStatus> AllowedStatuses { get; }

        public bool AllowWhenApprenticeshipQAFeatureDisabled { get; set; }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var featureFlagProvider = context.HttpContext.RequestServices.GetRequiredService<IFeatureFlagProvider>();
            var qaFeatureIsEnabled = featureFlagProvider.HaveFeature(FeatureFlags.ApprenticeshipQA);

            if (!qaFeatureIsEnabled && AllowWhenApprenticeshipQAFeatureDisabled)
            {
                await next();
                return;
            }

            var providerContextFeature = context.HttpContext.Features.Get<ProviderContextFeature>();

            if (providerContextFeature == null)
            {
                throw new InvalidOperationException("No provider context set.");
            }

            var providerId = providerContextFeature.ProviderInfo.ProviderId;

            var sqlQueryDispatcher = context.HttpContext.RequestServices.GetRequiredService<ISqlQueryDispatcher>();

            var currentStatus = await sqlQueryDispatcher.ExecuteQuery(
                new GetProviderApprenticeshipQAStatus()
                {
                    ProviderId = providerId
                });

            if (!AllowedStatuses.Contains(currentStatus ?? ApprenticeshipQAStatus.NotStarted))
            {
                context.Result = new BadRequestResult();
            }
            else
            {
                await next();
            }
        }
    }
}
