using System;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.Services
{
    public static class LoggerExtensions
    {
        public static void LogException<TCategoryName>(this ILogger<TCategoryName> logger, string message, Exception exception)
        {
            if (exception == null) throw new ArgumentNullException(nameof(exception));

            logger.LogError(exception, message);
        }
    }
}
