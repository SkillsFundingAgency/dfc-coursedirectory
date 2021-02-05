using System.Collections.Generic;
using System.Linq;

namespace Dfc.CourseDirectory.Functions.FixVenues
{
    public class AnalysisCounts
    {
        public int BatchSize { get; set; }
        public int CorruptLocationsAnalysed { get; private set; }
        public IList<FixCounts> FixCounts { get; private set; } = new List<FixCounts>();

        public static AnalysisCounts GetCounts(IList<ApprenticeshipVenueCorrection> apprenticeshipVenueCorrections)
        {
            var locationCorrections = apprenticeshipVenueCorrections
                .SelectMany(r => r.ApprenticeshipLocationVenueCorrections)
                .ToList();

            var fixCounts = locationCorrections.GroupBy(c => new {c.CorruptionType, c.UnfixableLocationVenueReason})
                .Select(x => new FixCounts(x.Key.CorruptionType, x.Key.UnfixableLocationVenueReason, x.Count()))
                .OrderBy(x => x.CorruptionType)
                .ThenBy(x => x.UnfixableLocationVenueReason)
                .ToList();

            return new AnalysisCounts
            {
                BatchSize = apprenticeshipVenueCorrections.Count(),
                CorruptLocationsAnalysed = locationCorrections.Count(),
                FixCounts = fixCounts,
            };
        }

        public AnalysisCounts Add(AnalysisCounts additionalCounts)
        {
            return new AnalysisCounts
            {
                BatchSize = BatchSize + additionalCounts.BatchSize,
                CorruptLocationsAnalysed = CorruptLocationsAnalysed + additionalCounts.CorruptLocationsAnalysed,
                // merge FixCount lists https://stackoverflow.com/questions/720609/create-a-list-from-two-object-lists-with-linq/6772832#6772832
                FixCounts = FixCounts.Concat(additionalCounts.FixCounts)
                    .ToLookup(counts => new {counts.CorruptionType, counts.UnfixableLocationVenueReason})
                    .Select(group => group.Aggregate((a, b) => new FixCounts(a.CorruptionType,
                        a.UnfixableLocationVenueReason ?? b.UnfixableLocationVenueReason, a.Count + b.Count))).ToList(),
            };
        }
    }
}
