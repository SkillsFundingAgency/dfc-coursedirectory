using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.DataStore.Sql;
using Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries;
using Dfc.CourseDirectory.WebV2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.WebV2.Filters
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
            var providerInfo = context.ActionArguments.Select(a => a.Value).OfType<ProviderInfo>().SingleOrDefault();
            if (providerInfo == null)
            {
                throw new InvalidOperationException(
                    $"Unknown provider. " +
                    $"This attribute can only be used when an action parameter of type {typeof(ProviderInfo).FullName} is defined.");
            }

            var sqlQueryDispatcher = context.HttpContext.RequestServices.GetRequiredService<ISqlQueryDispatcher>();
            var currentStatus = await sqlQueryDispatcher.ExecuteQuery(
                new GetProviderApprenticeshipQAStatus()
                {
                    ProviderId = providerInfo.ProviderId
                });

            if (!AllowedStatuses.Contains(currentStatus))
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
