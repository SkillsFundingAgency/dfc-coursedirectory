using System;
using System.Collections.Generic;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Dfc.CourseDirectory.Testing
{
    [TraitDiscoverer("Dfc.CourseDirectory.Testing.SlowTestDiscoverer", "Dfc.CourseDirectory.Testing")]
    public sealed class SlowTestAttribute : Attribute, ITraitAttribute
    {
    }

    public class SlowTestDiscoverer : ITraitDiscoverer
    {
        public IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute)
        {
            if (traitAttribute is ReflectionAttributeInfo reflectionAttributeInfo &&
                reflectionAttributeInfo.Attribute is SlowTestAttribute)
            {
                yield return new KeyValuePair<string, string>("SkipOnCI", "true");
                yield return new KeyValuePair<string, string>("SlowTest", "true");
            }
        }
    }
}
