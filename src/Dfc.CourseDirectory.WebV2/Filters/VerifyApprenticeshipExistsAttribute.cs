using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Dfc.CourseDirectory.WebV2.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class VerifyApprenticeshipExistsAttribute : ActionFilterAttribute
    {
        private const string DefaultParameterName = "apprenticeshipId";

        public VerifyApprenticeshipExistsAttribute(string parameterName)
        {
            ParameterName = parameterName ?? throw new ArgumentNullException(nameof(parameterName));
        }

        public VerifyApprenticeshipExistsAttribute()
            : this(DefaultParameterName)
        {
        }

        public string ParameterName { get; }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.ActionArguments.TryGetValue(ParameterName, out var apprenticeshipId))
            {
                throw new InvalidOperationException($"Action does not contain a parameter named '{ParameterName}'.");
            }

            if (!(apprenticeshipId is Guid))
            {
                throw new InvalidOperationException($"Apprenticeship ID parameter must be a GUID.");
            }

            var providerOwnershipCache = context.HttpContext.RequestServices.GetRequiredService<IProviderOwnershipCache>();
            var ukprn = await providerOwnershipCache.GetProviderForApprenticeship((Guid)apprenticeshipId);

            if (!ukprn.HasValue)
            {
                context.Result = new NotFoundResult();
            }
            else
            {
                await next();
            }
        }
    }
}
