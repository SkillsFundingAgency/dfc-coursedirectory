using System;
using Microsoft.AspNetCore.WebUtilities;

namespace Dfc.CourseDirectory.WebV2
{
    public class UrlBuilder
    {
        private string _url;

        public UrlBuilder(string baseUrl)
        {
            if (baseUrl == null)
            {
                throw new ArgumentNullException(nameof(baseUrl));
            }

            _url = baseUrl;
        }

        public static implicit operator string(UrlBuilder builder) => builder._url;

        public UrlBuilder AddQueryParameter(string name, object value)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            _url = QueryHelpers.AddQueryString(_url, name, value.ToString());

            return this;
        }

        public override string ToString() => _url;
    }
}
