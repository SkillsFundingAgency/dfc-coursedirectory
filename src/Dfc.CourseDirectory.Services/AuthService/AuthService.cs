using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Models.Models.Providers;
using Dfc.CourseDirectory.Services.Interfaces.AuthService;
using Dfc.CourseDirectory.Services.Interfaces.ProviderService;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.Services.AuthService
{
    public class AuthService : IAuthService
    {
        private readonly ILogger<AuthService> _logger;
        private readonly IProviderService _providerService;

        public AuthService(ILogger<AuthService> logger, IProviderService providerService)
        {
            _logger = logger;
            _providerService = providerService;
        }

        public async Task<string> GetProviderType(string UKPRN, string roleName)
        {
            if (roleName == "Provider User" || roleName == "Provider Superuser")
            {
                var providerDetails =
                    await _providerService.GetProviderByPRNAsync(UKPRN);

                var provider = providerDetails.Value?.FirstOrDefault();
                if (providerDetails.IsSuccess && provider != null)
                {
                    return provider.ProviderType.ToString();
                }
            }

            return ProviderType.Both.ToString();
        }
    }
}