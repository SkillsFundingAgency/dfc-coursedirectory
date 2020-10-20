using System.Collections.Generic;

namespace Dfc.CourseDirectory.Services.UnregulatedProvision
{
    public static class Categories
    {
        public static readonly IReadOnlyDictionary<string, string> AllCategories = new Dictionary<string, string>
        {
            { "APP H CAT A", "A: Non-regulated (ESFA funded)" },
            { "APP H CAT B", "B: English, maths, and ESOL (ESFA funded)" },
            { "APP H CAT E", "E: Not Community Learning and not ESFA funded" },
            { "APP H CAT F", "F: Community Learning" },
            { "APP H CAT G", "G: English, maths, and ESOL (not ESFA funded)" },
            { "APP H CAT I", "I: Work experience and work placement" },
            { "APP H CAT J", "J: Supported internship" },
            { "APP H CAT K", "K: Programme aim" },
            { "APP H CAT L", "L: ESFA co-financed" },
            { "APP H CAT M", "M: Conversion codes between HNQs" },
            { "APP H CAT O", "O: Education assessment" }
        };
    }
}