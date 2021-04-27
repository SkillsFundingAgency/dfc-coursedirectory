using System;

namespace Dfc.CourseDirectory.Core
{
    public static class ContentExtensions
    {
        public static string GetMessageForErrorCode(string errorCode, params object[] args)
        {
            var message = Content.ResourceManager.GetString(string.Format($"ERROR_{errorCode}", args));

            if (message == null)
            {
                throw new ArgumentException($"Cannot find error message for error code: {errorCode}.", nameof(errorCode));
            }

            return message;
        }
    }
}
