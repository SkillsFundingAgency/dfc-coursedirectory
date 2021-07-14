using System;
using System.Collections.Generic;
using System.Text;
using CsvHelper.Configuration.Attributes;

namespace Dfc.CourseDirectory.Core.DataManagement.Schemas
{
    public class CsvApprenticeshipRow
    {
        public const char SubRegionDelimiter = ';';

        [Index(0), Name("STANDARD_CODE")]
        public string StandardCode { get; set; }
        [Index(1), Name("STANDARD_VERSION")]
        public string StandardVersion { get; set; }
        [Index(2), Name("APPRENTICESHIP_INFORMATION")]
        public string ApprenticeshipInformation { get; set; }
    }
}
