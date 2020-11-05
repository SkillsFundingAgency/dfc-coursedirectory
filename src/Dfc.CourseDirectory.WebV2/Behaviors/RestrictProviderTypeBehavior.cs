using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Dfc.CourseDirectory.WebV2.Behaviors
{
    public class RestrictProviderTypeBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
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
            var providerId = _descriptor.GetProviderId(request);
            var providerInfo = await _providerInfoCache.GetProviderInfo(providerId);

            if (providerInfo == null)
            {
                throw new InvalidStateException(InvalidStateReason.ProviderDoesNotExist);
            }

            if (!providerInfo.ProviderType.HasFlag(_descriptor.ProviderType))
            {
                throw new InvalidStateException(InvalidStateReason.InvalidProviderType);
            }

            return await next();
        }
    }
}
