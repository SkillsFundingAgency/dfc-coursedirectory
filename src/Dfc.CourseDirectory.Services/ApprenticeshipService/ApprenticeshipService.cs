using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Common.Interfaces;
using Dfc.CourseDirectory.Models.Interfaces.Apprenticeships;
using Dfc.CourseDirectory.Services.Interfaces.ApprenticeshipService;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Services.ApprenticeshipService
{
    public class ApprenticeshipService : IApprenticeshipService
    {
        private readonly ILogger<ApprenticeshipService> _logger;
        private readonly HttpClient _httpClient;
        private readonly Uri _getStandardsAndFrameworksUri;


        public ApprenticeshipService(
            ILogger<ApprenticeshipService> logger,
            HttpClient httpClient,
            IOptions<ApprenticeshipServiceSettings> settings)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(httpClient, nameof(httpClient));
            Throw.IfNull(settings, nameof(settings));

            _logger = logger;
            _httpClient = httpClient;

            _getStandardsAndFrameworksUri = settings.Value.GetStandardsAndFrameworksUri();

        }

        public async Task<IResult<IEnumerable<IStandardsAndFrameworks>>> StandardsAndFrameworksSearch(string criteria)
        {
            Throw.IfNullOrWhiteSpace(criteria, nameof(criteria));
            _logger.LogMethodEnter();

            try
            {
                _logger.LogInformationObject("Standards and Frameworks Criteria", criteria);


                var response = await _httpClient.GetAsync(new Uri(_getStandardsAndFrameworksUri.AbsoluteUri + "&search=" + criteria));
                _logger.LogHttpResponseMessage("Standards and Frameworks service http response", response);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    _logger.LogInformationObject("Get your courses service json response", json);
                    var results = JsonConvert.DeserializeObject<IEnumerable<IStandardsAndFrameworks>>(json);

                    return Result.Ok<IEnumerable<IStandardsAndFrameworks>>(results);

                }
                else
                {
                    return Result.Fail<IEnumerable<IStandardsAndFrameworks>>("Standards and Frameworks service unsuccessful http response");
                }

            }
            catch (HttpRequestException hre)
            {
                _logger.LogException("Get your courses service http request error", hre);
                return Result.Fail<IEnumerable<IStandardsAndFrameworks>>("Standards and Frameworks service http request error.");

            }
            catch (Exception e)
            {
                _logger.LogException("Standards and Frameworks unknown error.", e);
                return Result.Fail<IEnumerable<IStandardsAndFrameworks>>("Standards and Frameworks service unknown error.");
            }
            finally
            {
                _logger.LogMethodExit();
            }
        }

    }
    internal static class ApprenticeshipServiceSettingsExtensions
    {
        internal static Uri GetStandardsAndFrameworksUri(this IApprenticeshipServiceSettings extendee)
        {
            return new Uri($"{extendee.ApiUrl + "StandardsAndFrameworksSearch?code=" + extendee.ApiKey}");
        }
    }
}
