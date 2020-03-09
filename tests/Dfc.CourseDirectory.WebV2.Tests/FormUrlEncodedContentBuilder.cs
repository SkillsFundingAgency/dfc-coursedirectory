﻿using System.Collections.Generic;
using System.Net.Http;

namespace Dfc.CourseDirectory.WebV2.Tests
{
    public class FormUrlEncodedContentBuilder
    {
        private readonly List<KeyValuePair<string, string>> _values;

        public FormUrlEncodedContentBuilder()
        {
            _values = new List<KeyValuePair<string, string>>();
        }

        public FormUrlEncodedContentBuilder With(string key, object value)
        {
            _values.Add(new KeyValuePair<string, string>(key, value.ToString()));

            return this;
        }

        public FormUrlEncodedContent ToContent() => new FormUrlEncodedContent(_values);
    }
}
