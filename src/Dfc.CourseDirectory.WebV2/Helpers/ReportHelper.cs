using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using CsvHelper;

namespace Dfc.CourseDirectory.WebV2.Helpers
{
    public class ReportHelper
    {
        public static byte[]  ConvertToBytes(Features.ApprenticeshipQA.Report.ViewModel model)
        {
            using (var logStream = new MemoryStream())
            using (var logStreamWriter = new StreamWriter(logStream))
            using (var logCsvWriter = new CsvWriter(logStreamWriter, CultureInfo.InvariantCulture))
            {
                logCsvWriter.WriteRecords(model.Report);
                logStreamWriter.Flush();
                logStream.Seek(0L, SeekOrigin.Begin);
                return logStream.ToArray();

            }
        }
    }
}
