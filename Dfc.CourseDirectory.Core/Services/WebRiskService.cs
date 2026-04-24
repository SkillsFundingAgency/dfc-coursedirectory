using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;

namespace Dfc.CourseDirectory.Core.Services
{
    public class WebRiskService : IWebRiskService
    {
        private readonly GoogleWebRiskSettings _WebRiskSettings;
        private readonly IHttpClientFactory _factory;
        private readonly ILogger<WebRiskService> _logger;
        private readonly IHostEnvironment _hostEnvironment;

        public WebRiskService(
            IOptions<GoogleWebRiskSettings> options,
            IHttpClientFactory factory,
            IHostEnvironment hostEnvironment)
        {
            _WebRiskSettings = options.Value;
            _factory = factory;
            _hostEnvironment = hostEnvironment;

            _logger = LoggerFactory.Create(builder => builder.AddConsole())
                                   .CreateLogger<WebRiskService>();
        }

        public async Task<bool> CheckForSecureUri(string url)
        {

            var currentEnvironment = _hostEnvironment.EnvironmentName;

            bool isNonProduction =
                !string.Equals(currentEnvironment, "Production", StringComparison.OrdinalIgnoreCase);

            if (_WebRiskSettings.PerformanceTesting && isNonProduction)
            {
                _logger.LogInformation(
                    "WebRiskService: Performance testing enabled in non-production environment ({env}), skipping external service call",
                    currentEnvironment
                );
                return true;
            }

            using (var client = _factory.CreateClient("namedClient"))
            {
                try
                {
                    string prefix = "https://webrisk.googleapis.com/v1/uris:search?";
                    string key = _WebRiskSettings.ApiKey;
                    string threat_types = "&threatTypes=MALWARE&threatTypes=SOCIAL_ENGINEERING&threatTypes=UNWANTED_SOFTWARE";

                    string query = $"{prefix}key={key}{threat_types}&uri={HttpUtility.UrlEncode(url)}";
                    var result = await client.GetAsync(query);       
                    var json = await result.Content.ReadAsStringAsync();

                    if (result.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        if (json.Contains("threat"))
                        {
                            _logger.LogInformation("WebRiskService: Found 1 or more threats");
                            return false;
                        }
                        else
                        {
                            // empty json is returned when no threat is found
                            _logger.LogInformation("WebRiskService: Found 0 threats");
                            return true;
                        }
                    } else
                    {
                        //status not OK
                        _logger.LogInformation("WebRiskService: Service unavailable or invalid request");
                        return false;
                    }
                }
                catch
                {
                    _logger.LogInformation("WebRiskService: Could not connect to the external service");
                    return false;
                }
            }
        }
    }
}
