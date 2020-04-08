using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.WebV2.Tests
{
    public static class CsvHelperExtensions
    {
        public static async Task<IEnumerable<T>> AsCsvListOf<T>(this HttpResponseMessage response)
        {
            using (var reader = new StreamReader(await response.Content.ReadAsStreamAsync()))
            using (var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csvReader.GetRecords<T>();
                return records.ToList();
            }
        }
    }
}
