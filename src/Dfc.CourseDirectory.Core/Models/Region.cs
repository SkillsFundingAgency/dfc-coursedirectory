using System;
using System.Collections.Generic;
using System.Linq;

namespace Dfc.CourseDirectory.Core.Models
{
    public class Region
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public IReadOnlyCollection<Region> SubRegions { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        /// <summary>
        /// Combine sub regions into their parent region(s) when all sub regions for that parent region
        /// are specified.
        /// </summary>
        public static IReadOnlyCollection<Region> Reduce(
            IReadOnlyCollection<Region> allRegions,
            IEnumerable<string> regionIds)
        {
            if (allRegions is null)
            {
                throw new ArgumentNullException(nameof(allRegions));
            }

            if (regionIds is null)
            {
                throw new ArgumentNullException(nameof(regionIds));
            }

            var regionIdsArray = regionIds.Distinct().ToArray();

            var topLevelRegionsById = allRegions.ToDictionary(r => r.Id, r => r);

            var subRegionsWithParentId = allRegions.SelectMany(r => r.SubRegions.Select(sr => (ParentRegion: r, SubRegion: sr)))
                .ToDictionary(r => r.SubRegion.Id, r => r);

            var result = new List<Region>();

            foreach (var id in regionIdsArray)
            {
                if (topLevelRegionsById.TryGetValue(id, out var region))
                {
                    // Top-level region
                    result.Add(region);
                }
                else if (subRegionsWithParentId.TryGetValue(id, out var subRegionWithParent))
                {
                    // Sub region

                    if (result.Contains(subRegionWithParent.ParentRegion))
                    {
                        // We already have the parent region
                        continue;
                    }
                    else if (subRegionWithParent.ParentRegion.SubRegions.All(sr => regionIdsArray.Contains(sr.Id)))
                    {
                        // We have every sub region for this root region specified
                        result.Add(subRegionWithParent.ParentRegion);
                    }
                    else
                    {
                        result.Add(subRegionWithParent.SubRegion);
                    }
                }
                else
                {
                    throw new ArgumentException($"Unknown region: '{id}'.");
                }
            }

            return result;
        }
    }
}
