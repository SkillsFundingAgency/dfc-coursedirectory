using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Dfc.CourseDirectory.Services.CourseService
{
    public class CourseService : ICourseService
    {
        private readonly ILogger<CourseService> _logger;
        private readonly HttpClient _httpClient;
        private readonly Uri _getWhatEverUri;

        public CourseService(
            ILogger<CourseService> logger,
            HttpClient httpClient,
            IOptions<CourseServiceSettings> settings)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(httpClient, nameof(httpClient));
            Throw.IfNull(settings, nameof(settings));

            _logger = logger;
            _httpClient = httpClient;

            _getWhatEverUri = settings.Value.ToGetWhatEverUri();
        }

        // TODO - some real work methods;
    }

    internal static class CourseServiceSettingsExtensions
    {
        internal static Uri ToGetWhatEverUri(this ICourseServiceSettings extendee)
        {
            return new Uri($"{extendee.ApiUrl + "GetWhatEver?code=" + extendee.ApiKey}");
        }
    }
}
