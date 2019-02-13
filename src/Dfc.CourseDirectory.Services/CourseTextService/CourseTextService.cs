﻿using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Common.Interfaces;
using Dfc.CourseDirectory.Models.Interfaces.Courses;
using Dfc.CourseDirectory.Models.Models.Courses;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.CourseTextService;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.Services.CourseTextService
{
    public class CourseTextService : ICourseTextService
    {
        private readonly ILogger<CourseTextService> _logger;
        private readonly HttpClient _httpClient;
        private readonly Uri _getYourCourseTextUri;


        public CourseTextService(
            ILogger<CourseTextService> logger,
            HttpClient httpClient,
            IOptions<CourseTextServiceSettings> settings)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(httpClient, nameof(httpClient));
            Throw.IfNull(settings, nameof(settings));

            _logger = logger;
            _httpClient = httpClient;

            _getYourCourseTextUri = settings.Value.GetCourseTextUri();

        }

        public async Task<IResult<ICourseText>> GetCourseTextByLARS(ICourseTextSearchCriteria criteria)
        {
            Throw.IfNull(criteria, nameof(criteria));
            Throw.IfNullOrWhiteSpace(criteria.LARSRef, nameof(criteria.LARSRef));
            _logger.LogMethodEnter();

            try
            {
                _logger.LogInformationObject("Course Text Criteria", criteria);
                _logger.LogInformationObject("Course Text URI", _getYourCourseTextUri);

                if (criteria.LARSRef == "")
                    return Result.Fail<ICourseText>("Blank LARS Ref");

                var response = await _httpClient.GetAsync(new Uri(_getYourCourseTextUri.AbsoluteUri + "&LARS=" + criteria.LARSRef));
                _logger.LogHttpResponseMessage("Get your courses service http response", response);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    _logger.LogInformationObject("Get your courses service json response", json);
                    var courseText = JsonConvert.DeserializeObject<CourseText>(json);

                    return Result.Ok<ICourseText>(courseText);

                }
                else
                {
                    return Result.Fail<ICourseText>("Get your courses service unsuccessful http response");
                }

            }
            catch (HttpRequestException hre)
            {
                _logger.LogException("Get your courses service http request error", hre);
                return Result.Fail<ICourseText>("Get your courses service http request error.");

            }
            catch (Exception e)
            {
                _logger.LogException("Get your courses service unknown error.", e);
                return Result.Fail<ICourseText>("Get your courses service unknown error.");
            }
            finally
            {
                _logger.LogMethodExit();
            }
        }

    }
    internal static class CourseServiceSettingsExtensions
    {
        internal static Uri GetCourseTextUri(this ICourseTextServiceSettings extendee)
        {
            return new Uri($"{extendee.ApiUrl + "GetCourseTextByLARS?code=" + extendee.ApiKey}");
        }
    }
}
