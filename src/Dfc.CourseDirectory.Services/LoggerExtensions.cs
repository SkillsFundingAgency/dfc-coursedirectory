﻿using System;
using System.Diagnostics;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Dfc.CourseDirectory.Services
{
    public static class LoggerExtensions
    {
        public static void LogHttpResponseMessage<TCategoryName>(this ILogger<TCategoryName> logger, string message, HttpResponseMessage httpResponseMessage)
        {
            if (httpResponseMessage == null) throw new ArgumentNullException(nameof(httpResponseMessage));

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                logger.LogInformationObject(message, httpResponseMessage);
            }
            else
            {
                logger.LogWarning(message + " {0}", JsonConvert.SerializeObject(httpResponseMessage));
            }
        }

        public static void LogException<TCategoryName>(this ILogger<TCategoryName> logger, string message, Exception exception)
        {
            if (exception == null) throw new ArgumentNullException(nameof(exception));

            logger.LogError(exception, message);
        }

        public static void LogException<TCategoryName>(this ILogger<TCategoryName> logger, Exception exception)
        {
            if (exception == null) throw new ArgumentNullException(nameof(exception));

            logger.LogException(string.Empty + " {0}", exception);
        }

        public static void LogInformationObject<TCategoryName>(this ILogger<TCategoryName> logger, string message, object obj)
        {
            string json;

            if (obj is string)
            {
                var maxLength = 4096; // This may change but it is a best guess for now.
                json = ((string)obj)?.Substring(0, Math.Min(((string)obj).Length, maxLength));
            }
            else
            {
                json = JsonConvert.SerializeObject(obj);
            }

            logger.LogInformation(message + " {0}", json);
        }
    }
}