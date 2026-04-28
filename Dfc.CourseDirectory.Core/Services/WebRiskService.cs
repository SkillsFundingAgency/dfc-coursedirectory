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

        private readonly IHttpContextAccessor _httpContextAccessor;

        public WebRiskService(IOptions<GoogleWebRiskSettings> options, IHttpClientFactory factory, IHttpContextAccessor httpContextAccessor)
        {
            _WebRiskSettings = options.Value;
            _factory = factory;
            _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<WebRiskService>();
            _httpContextAccessor = httpContextAccessor;
        }

        // Add this constructor to match the usage in your test file
        public WebRiskService(
            IOptions<GoogleWebRiskSettings> webRiskSettings,
            IHttpClientFactory factory,
            ILogger<WebRiskService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _WebRiskSettings = webRiskSettings?.Value ?? throw new ArgumentNullException(nameof(webRiskSettings));
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public async Task<bool> CheckForSecureUri(string url)
        {

            var request = _httpContextAccessor.HttpContext?.Request;

            var requestUrl = request != null
                ? $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}"
                : string.Empty;

            var uri = new Uri(requestUrl);
            var host = uri.Host; 

              var environment = host.Split('.')[0]; 

           var allowedEnvironments = _WebRiskSettings.PerformanceTestingAllowedEnvironments?
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                ?? Array.Empty<string>();

            bool isAllowedEnvironment = allowedEnvironments.Contains(
                environment,
                StringComparer.OrdinalIgnoreCase
            );

            if (_WebRiskSettings.PerformanceTesting && isAllowedEnvironment)
            {
                _logger.LogInformation(
                    "WebRiskService: Performance testing enabled in allowed environment ({env}), skipping external service call",
                    requestUrl
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
