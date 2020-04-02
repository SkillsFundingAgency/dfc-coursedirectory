using System;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.WebV2
{
    public static class EnumHelper
    {
        public static IReadOnlyCollection<T> SplitFlags<T>(this T value)
            where T : Enum, IConvertible
        {
            var valueAsInt = value.ToInt32(provider: null);

            var split = new List<T>();

            foreach (T r in Enum.GetValues(typeof(T)))
            {
                var optionAsInt = r.ToInt32(provider: null);

                // If the value is not a power of two treat it as a combination of other options
                // and don't return it
                if ((optionAsInt != 0) && ((optionAsInt & (optionAsInt - 1)) == 0))
                {
                    continue;
                }

                if ((valueAsInt & optionAsInt) != 0)
                {
                    split.Add(r);
                }
            }

            return split;
        }
    }
}
