namespace Dfc.CourseDirectory.Functions.FixVenues
{
    internal class FixCounts
    {
        public CorruptionType CorruptionType { get; }

        /// <summary>
        /// Null if fixable
        /// </summary>
        public UnfixableLocationVenueReasons? UnfixableLocationVenueReason { get; }

        public int Count { get; }

        public FixCounts(CorruptionType corruptionType, UnfixableLocationVenueReasons? unfixableLocationVenueReason, int count)
        {
            CorruptionType = corruptionType;
            UnfixableLocationVenueReason = unfixableLocationVenueReason;
            Count = count;
        }
    }
}
