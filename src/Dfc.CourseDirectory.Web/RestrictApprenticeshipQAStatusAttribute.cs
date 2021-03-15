using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.WebV2;
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
        }

        public ISet<ApprenticeshipQAStatus> AllowedStatuses { get; }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var providerContextProvider = context.HttpContext.RequestServices.GetRequiredService<IProviderContextProvider>();
            var providerContext = providerContextProvider.GetProviderContext(withLegacyFallback: true);

            if (providerContext == null)
            {
                throw new InvalidOperationException("No provider context set.");
            }

            var providerId = providerContext.ProviderInfo.ProviderId;

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
