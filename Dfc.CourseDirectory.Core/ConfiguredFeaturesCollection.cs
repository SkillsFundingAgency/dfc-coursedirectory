using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Dfc.CourseDirectory.Core
{
    public class ConfiguredFeaturesCollection : IReadOnlyCollection<string>
    {
        private readonly HashSet<string> _features;

        public ConfiguredFeaturesCollection()
            : this(new string[0])
        {
        }

        public ConfiguredFeaturesCollection(params string[] features)
            : this(features.AsEnumerable())
        {
        }

        public ConfiguredFeaturesCollection(IEnumerable<string> features)
        {
            if (features is null)
            {
                throw new ArgumentNullException(nameof(features));
            }

            _features = new HashSet<string>(features, StringComparer.OrdinalIgnoreCase);
        }

        public int Count => _features.Count;

        public IEnumerator<string> GetEnumerator() => _features.GetEnumerator();

        public bool HaveFeature(string feature)
        {
            if (feature is null)
            {
                throw new ArgumentNullException(nameof(feature));
            }

            return _features.Contains(feature);
        }

        public ConfiguredFeaturesCollection With(string feature)
        {
            if (feature is null)
            {
                throw new ArgumentNullException(nameof(feature));
            }

            return new ConfiguredFeaturesCollection(_features.Append(feature));
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
