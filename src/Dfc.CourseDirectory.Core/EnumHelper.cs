﻿using System;
using System.Collections.Generic;

namespace Dfc.CourseDirectory.Core
{
    public static class EnumHelper
    {
        public static bool HasFlags<T>(this T value, params T[] flags)
            where T : Enum, IConvertible
        {
            var valueAsInt = value.ToInt32(provider: null);

            foreach (var flag in flags)
            {
                var flagAsInt = flag.ToInt32(provider: null);

                if ((valueAsInt & flagAsInt) == 0)
                {
                    return false;
                }
            }

            return true;
        }

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
                if (!(optionAsInt != 0) && ((optionAsInt & (optionAsInt - 1)) == 0))
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

        public static IReadOnlyCollection<T> ProviderTypeSplitFlags<T>(this T value)
            where T : Enum, IConvertible
        {
            var valueAsInt = value.ToInt32(provider: null);

            var split = new List<T>();

            foreach (T r in Enum.GetValues(typeof(T)))
            {
                var optionAsInt = r.ToInt32(provider: null);

                // If the value is not a power of two treat it as a combination of other options
                // and don't return it
                if (valueAsInt == optionAsInt && (valueAsInt == 1 || valueAsInt == 4)) split.Add(r);

                if (valueAsInt == 5)
                {
                    if (optionAsInt == 1 || optionAsInt == 4)
                        split.Add(r);
                }

            }

            return split;
        }
    }
}
