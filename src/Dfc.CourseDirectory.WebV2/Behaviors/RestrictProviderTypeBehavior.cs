using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Behaviors.Errors;
using MediatR;

namespace Dfc.CourseDirectory.WebV2.Behaviors
{
    public class RestrictProviderTypeBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IProviderScopedRequest
    {
        private readonly IRestrictProviderType<TRequest> _descriptor;
        private readonly IProviderInfoCache _providerInfoCache;

        public RestrictProviderTypeBehavior(
            IRestrictProviderType<TRequest> descriptor,
            IProviderInfoCache providerInfoCache)
        {
            _descriptor = descriptor;
            _providerInfoCache = providerInfoCache;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            CancellationToken cancellationToken,
            RequestHandlerDelegate<TResponse> next)
        {
            var providerId = request.ProviderId;
            var providerInfo = await _providerInfoCache.GetProviderInfo(providerId);

            if (providerInfo == null)
            {
                throw new ErrorException<ProviderDoesNotExist>(new ProviderDoesNotExist());
            }

            if (!providerInfo.ProviderType.HasFlag(_descriptor.ProviderType))
            {
                throw new ErrorException<InvalidProviderType>(new InvalidProviderType());
            }

            return await next();
        }
    }
}
