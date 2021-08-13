using System.Collections.Generic;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
using Dfc.CourseDirectory.Core.Models;
using Mapster;

namespace Dfc.CourseDirectory.Core.DataManagement
{
    public class ParsedCsvApprenticeshipRow : CsvApprenticeshipRow
    {
        private const string DateFormat = "dd/MM/yyyy";
        public ApprenticeshipDeliveryMode? ResolvedDeliveryMode { get; private set; }
        public IReadOnlyCollection<Region> ResolvedSubRegions { get; private set; }
        public ApprenticeshipLocationType? ResolvedDeliveryMethod { get; private set; }
        public bool? ResolvedNationalDelivery { get; private set; }

        private ParsedCsvApprenticeshipRow()
        {
        }

        public static ParsedCsvApprenticeshipRow FromCsvApprenticeshipRow(CsvApprenticeshipRow row)
        {
            var parsedRow = row.Adapt(new ParsedCsvApprenticeshipRow());
            return parsedRow;
        }
    }
}
