using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Dfc.CourseDirectory.Core.ReferenceData.Ukrlp;
using CsvHelper;

namespace Dfc.CourseDirectory.Core.Services
{
    public class WebRiskService : IWebRiskService
    {
        private readonly GoogleWebRiskSettings _WebRiskSettings;
        private readonly IHttpClientFactory _factory;
        private readonly ILogger<WebRiskService> _logger;

        public WebRiskService(IOptions<GoogleWebRiskSettings> options, IHttpClientFactory factory)
        {
            _WebRiskSettings = options.Value;
            _factory = factory;
            _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<WebRiskService>(); ;
        }

        public async Task<bool> CheckForSecureUri(string url)
        {
            using (var client = _factory.CreateClient())
            {
                try
                {
                    string prefix = "https://webrisk.googleapis.com/v1/uris:search?";
                    string key = _WebRiskSettings.ApiKey;
                    string threat_types = "&threatTypes=MALWARE&threatTypes=SOCIAL_ENGINEERING&threatTypes=UNWANTED_SOFTWARE";

                    string query = $"{prefix}key={key}{threat_types}&uri={url}";
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
