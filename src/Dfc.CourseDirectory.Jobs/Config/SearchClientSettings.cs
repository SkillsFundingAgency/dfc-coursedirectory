using System;

namespace Dfc.CourseDirectory.Jobs.Config
{
    public class SearchClientSettings
    {
        public string Url { get; }

        public string Key { get; }

        public string IndexName { get; }

        public SearchClientSettings(string url, string key, string indexName)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (string.IsNullOrWhiteSpace(indexName))
            {
                throw new ArgumentNullException(nameof(indexName));
            }

            Url = url;
            Key = key;
            IndexName = indexName;
        }
    }
}