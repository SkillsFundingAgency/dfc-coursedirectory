using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
using Dfc.CourseDirectory.Core.Models;
using Mapster;

namespace Dfc.CourseDirectory.Core.DataManagement
{
    public class ParsedCsvApprenticeshipRow : CsvApprenticeshipRow
    {
        private const string DateFormat = "dd/MM/yyyy";

        private ParsedCsvApprenticeshipRow()
        {
        }

        public static ParsedCsvApprenticeshipRow FromCsvCourseRow(CsvApprenticeshipRow row)
        {
            var parsedRow = row.Adapt(new ParsedCsvApprenticeshipRow());
            return parsedRow;
        }
    }
}
