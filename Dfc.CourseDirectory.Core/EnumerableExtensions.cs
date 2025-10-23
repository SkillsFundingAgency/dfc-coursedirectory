using System;
using System.Collections.Generic;
using System.Linq;

namespace Dfc.CourseDirectory.Core
{
    public static class EnumerableExtensions
    {
        public static string ToCommaSeparatedString(
            this IEnumerable<string> values,
            string finalValuesConjunction = "and")
        {
            if (values is null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            if (finalValuesConjunction is null)
            {
                throw new ArgumentNullException(nameof(finalValuesConjunction));
            }

            var valuesArray = values.ToArray();

            if (valuesArray.Length == 0)
            {
                return string.Empty;
            }
            else if (valuesArray.Length == 1)
            {
                return valuesArray[0];
            }
            else
            {
                return string.Join(", ", valuesArray[0..^2].Append(string.Join($" {finalValuesConjunction} ", valuesArray[^2..])));
            }
        }
    }
}
