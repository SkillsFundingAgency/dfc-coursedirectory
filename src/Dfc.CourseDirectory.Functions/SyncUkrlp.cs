using Dfc.CourseDirectory.Core.ReferenceData.Ukrlp;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Dfc.CourseDirectory.Functions
{
    public class SyncUkrlp
    {
        private readonly UkrlpSyncHelper _ukrlpSyncHelper;
        private readonly ILogger<SyncUkrlp> _logger;

        public SyncUkrlp(UkrlpSyncHelper ukrlpSyncHelper, ILogger<SyncUkrlp> logger)
        {
            _ukrlpSyncHelper = ukrlpSyncHelper;
            _logger = logger;
        }

        // Select provider records to be synced (those modified in the past 7 days)
        [Function("SyncAllRecentlyModifiedProviders")]
        public async Task SyncAllRecentlyModifiedProviders([TimerTrigger("0 0 3 * * *")] TimerInfo timer)
        {
            _logger.LogInformation("Function '{0}' was invoked", nameof(SyncAllRecentlyModifiedProviders));

            DateTime updatedSince = DateTime.Today.AddDays(-7);
            
            _logger.LogInformation("Starting UKRLP Sync using cutoff date of {0}", updatedSince);

            await _ukrlpSyncHelper.SyncAllProviderData(updatedSince);

            _logger.LogInformation("Function '{0}' finished invoking", nameof(SyncAllRecentlyModifiedProviders));
        }

        [Function("SyncAllProviders")]
        public Task SyncAllProviders([HttpTrigger(AuthorizationLevel.Function, "get", "post")] string input) => _ukrlpSyncHelper.SyncAllKnownProvidersData();

        /*
            Trigger the function using the following request body:
            
            {
                "Ukprn": <int>
            }
        */
        [Function("SyncProviderByUkprn")]
        public async Task<IActionResult> SyncProviderByUkprn([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("Function '{0}' was invoked", nameof(SyncProviderByUkprn));

            using StreamReader reader = new(req.Body);
            string bodyStr = await reader.ReadToEndAsync();

            ProviderToRefresh providerData = JsonConvert.DeserializeObject<ProviderToRefresh>(bodyStr);

            _logger.LogInformation("UKPRN provided: {0}", providerData.Ukprn);

            await _ukrlpSyncHelper.SyncProviderData(providerData.Ukprn);

            _logger.LogInformation("Function '{0}' finished invoking", nameof(SyncProviderByUkprn));

            return new OkResult();
        }

        public class ProviderToRefresh
        {
            public int Ukprn;
        }
    }
}
