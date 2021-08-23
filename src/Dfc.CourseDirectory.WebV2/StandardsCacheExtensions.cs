using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.WebV2
{
    public static class StandardsCacheExtensions
    {
        private static readonly Regex _searchableChars =
            new Regex("[^a-z0-9]", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static async Task<IReadOnlyCollection<Standard>> SearchStandards(
            this IStandardsCache cache,
            string search)
        {
            var all = await cache.GetAllStandards();

            var formattedSearchTerm = FormatSearchTerm(search);

            return all
                .Where(s => IsMatch(formattedSearchTerm, FormatSearchTerm(s.StandardName)))
                .OrderBy(s => s.StandardName)
                .ToList();
        }

        private static IReadOnlyCollection<string> FormatSearchTerm(string searchTerm)
        {
            var split = _searchableChars.Replace(searchTerm, " ")
                .ToLower()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            return split;
        }

        private static bool IsMatch(IReadOnlyCollection<string> searchWords, IReadOnlyCollection<string> referenceWords)
        {
            // Match whenever any search term is found in reference words using a prefix match
            // i.e. search for 'retail' should match 'retail' & 'retailer'
            // but search for 'etail' should not match either
            // *unless* the search term is a number in which case the entire word must match

            return searchWords.Any(s => referenceWords.Any(r => s.All(Char.IsDigit) ? r.Equals(s) : r.StartsWith(s)));
        }
    }
}
