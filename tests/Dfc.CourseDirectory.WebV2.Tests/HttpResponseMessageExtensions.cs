using System;
using System.Net.Http;

namespace Dfc.CourseDirectory.WebV2.Tests
{
    public static class HttpResponseMessageExtensions
    {
        /// <summary>
        /// Throws an exception if the <see cref="HttpResponseMessage.StatusCode"/> is
        /// in the client errors (400-499) or server errors (500-599) class.
        /// </summary>
        /// <param name="response">The response message.</param>
        public static void EnsureNonErrorStatusCode(this HttpResponseMessage response)
        {
            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            if ((int)response.StatusCode >= 400)
            {
                response.EnsureSuccessStatusCode();
            }
        }
    }
}
