using Dfc.CourseDirectory.Core.DataManagement.Schemas;
using Mapster;

namespace Dfc.CourseDirectory.Core.DataManagement
{
    public class ParsedCsvApprenticeshipRow : CsvApprenticeshipRow
    {
        private const string DateFormat = "dd/MM/yyyy";

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
