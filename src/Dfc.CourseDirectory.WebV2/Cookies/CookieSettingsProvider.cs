﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace Dfc.CourseDirectory.WebV2.Cookies
{
    public class CookieSettingsProvider : ICookieSettingsProvider
    {
        private const string CookieName = "cookie-settings";

        private static readonly TimeSpan _cookieExpiry = TimeSpan.FromDays(365);

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _environment;

        public CookieSettingsProvider(
            IHttpContextAccessor httpContextAccessor,
            IWebHostEnvironment environment)
        {
            _httpContextAccessor = httpContextAccessor;
            _environment = environment;
        }

        public CookieSettings GetPreferencesForCurrentUser()
        {
            var request = _httpContextAccessor.HttpContext.Request;
            var cookieValue = request.Cookies[CookieName];

            return cookieValue != null && TryDeserializeSettings(cookieValue, out var settings) ?
                settings :
                null;
        }

        public void SetPreferencesForCurrentUser(CookieSettings preferences)
        {
            var response = _httpContextAccessor.HttpContext.Response;

            response.OnStarting(state =>
            {
                var (rsp, settings) = ((HttpResponse, CookieSettings))state;

                var serializedSettings = SerializeCookieSettings(settings);

                // N.B. SameSite cannot be Strict here otherwise the cookie
                // will not be sent immediately after sign in
                // (since the request originated from a different domain)

                rsp.Cookies.Append(CookieName, serializedSettings, new CookieOptions()
                {
                    Expires = DateTime.Now.Add(_cookieExpiry),
                    HttpOnly = true,
                    Secure = _environment.IsProduction(),
                    SameSite = SameSiteMode.Lax
                });

                return Task.CompletedTask;
            }, (response, preferences));
        }

        private static bool TryDeserializeSettings(string value, out CookieSettings settings)
        {
            try
            {
                settings = JsonConvert.DeserializeObject<CookieSettings>(value);
                return true;
            }
            catch (JsonException)
            {
                settings = default;
                return false;
            }
        }

        private static string SerializeCookieSettings(CookieSettings settings) => JsonConvert.SerializeObject(settings);
    }
}
