using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.WebV2.Security;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.WebV2.Filters
{
    public class AuthorizeApprenticeshipQASubmissionAttribute : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var services = context.HttpContext.RequestServices;
            var currentUserProvider = services.GetRequiredService<ICurrentUserProvider>();
            var providerContextProvider = services.GetRequiredService<IProviderContextProvider>();
            var sqlQueryDispatcher = services.GetRequiredService<ISqlQueryDispatcher>();
            var providerInfoCache = services.GetRequiredService<IProviderInfoCache>();

            var providerContext = providerContextProvider.GetProviderContext();

            if (providerContext == null)
            {
                throw new InvalidOperationException("No provider context set.");
            }

            var providerId = providerContext.ProviderInfo.ProviderId;

            var currentUser = currentUserProvider.GetCurrentUser();

            if (!AuthorizationRules.CanSubmitQASubmission(currentUser, providerId))
            {
                throw new NotAuthorizedException();
            }

            var qaStatus = await sqlQueryDispatcher.ExecuteQuery(
                new GetProviderApprenticeshipQAStatus()
                {
                    ProviderId = providerId
                });

            var effectiveQaStatus = qaStatus.ValueOrDefault();

            // Ignore UnableToComplete here
            var qaStatusIsValid = (effectiveQaStatus & ~ApprenticeshipQAStatus.UnableToComplete) switch
            {
                ApprenticeshipQAStatus.NotStarted => true,
                ApprenticeshipQAStatus.Failed => true,
                _ => false
            };

            var providerInfo = await providerInfoCache.GetProviderInfo(providerId);
            var providerTypeIsValid = providerInfo.ProviderType.HasFlag(ProviderType.Apprenticeships);

            if (!qaStatusIsValid || !providerTypeIsValid)
            {
                throw new InvalidStateException(InvalidStateReason.InvalidApprenticeshipQAStatus);
            }

            await next();
        }
    }
}
