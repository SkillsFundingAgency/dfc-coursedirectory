using System.Collections.Generic;
using System.Net.Http;

namespace Dfc.CourseDirectory.Testing
{
    public class FormUrlEncodedContentBuilder
    {
        private readonly List<KeyValuePair<string, string>> _values;

        public FormUrlEncodedContentBuilder()
        {
            _values = new List<KeyValuePair<string, string>>();
        }

        public FormUrlEncodedContentBuilder Add(string key, object value)
        {
            if (value != null)
            {
                _values.Add(new KeyValuePair<string, string>(key, value.ToString()));
            }

            return this;
        }

        public FormUrlEncodedContentBuilder Add(string key, string value) =>
            Add(key, (object)value);

        public FormUrlEncodedContentBuilder Add<T>(string key, IEnumerable<T> values)
        {
            if (values != null)
            {
                foreach (var value in values)
                {
                    _values.Add(new KeyValuePair<string, string>(key, value.ToString()));
                }
            }

            return this;
        }

        public FormUrlEncodedContent ToContent() => new FormUrlEncodedContent(_values);
    }
}
