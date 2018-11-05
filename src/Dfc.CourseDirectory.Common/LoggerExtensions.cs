using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Net.Http;

namespace Dfc.CourseDirectory.Common
{
    public static class LoggerExtensions
    {
        public static void LogHttpResponseMessage<TCategoryName>(this ILogger<TCategoryName> logger, string message, HttpResponseMessage httpResponseMessage)
        {
            if (httpResponseMessage == null) throw new ArgumentNullException(nameof(httpResponseMessage));

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                logger.LogInformation(message, httpResponseMessage);
            }
            else
            {
                logger.LogWarning(message, httpResponseMessage);
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

            logger.LogException(string.Empty, exception);
        }

        public static void LogMethodEnter<TCategoryName>(this ILogger<TCategoryName> logger)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                var method = new StackFrame(1).GetMethod();
                logger.LogDebug("Entering method.", method.ToString());
            }
        }

        public static void LogMethodExit<TCategoryName>(this ILogger<TCategoryName> logger)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                var method = new StackFrame(1).GetMethod();
                logger.LogDebug("Exiting method.", method.ToString());
            }
        }
    }
}