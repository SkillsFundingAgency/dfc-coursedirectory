using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using Dfc.CourseDirectory.Core.DataManagement;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;

namespace Dfc.CourseDirectory.Testing
{
    public static class DataManagementFileHelper
    {
        public static Stream CreateVenueUploadCsvStream(Action<CsvWriter> writeRows, bool writeHeader = true)
        {
            // N.B. We deliberately do not use the VenueRow class here to ensure we notice if any columns change name

            var stream = new MemoryStream();

            using (var streamWriter = new StreamWriter(stream, leaveOpen: true))
            using (var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
            {
                if (writeHeader)
                {
                    csvWriter.WriteField("YOUR_VENUE_REFERENCE");
                    csvWriter.WriteField("VENUE_NAME");
                    csvWriter.WriteField("ADDRESS_LINE_1");
                    csvWriter.WriteField("ADDRESS_LINE_2");
                    csvWriter.WriteField("TOWN_OR_CITY");
                    csvWriter.WriteField("COUNTY");
                    csvWriter.WriteField("POSTCODE");
                    csvWriter.WriteField("EMAIL");
                    csvWriter.WriteField("PHONE");
                    csvWriter.WriteField("WEBSITE");
                    csvWriter.NextRecord();
                }

                writeRows(csvWriter);
            }

            stream.Seek(0L, SeekOrigin.Begin);

            return stream;
        }

        public static Stream CreateVenueUploadCsvStream(params CsvVenueRow[] rows) => CreateVenueUploadCsvStream(csvWriter =>
        {
            foreach (var row in rows)
            {
                csvWriter.WriteField(row.ProviderVenueRef);
                csvWriter.WriteField(row.VenueName);
                csvWriter.WriteField(row.AddressLine1);
                csvWriter.WriteField(row.AddressLine2);
                csvWriter.WriteField(row.Town);
                csvWriter.WriteField(row.County);
                csvWriter.WriteField(row.Postcode);
                csvWriter.WriteField(row.Email);
                csvWriter.WriteField(row.Telephone);
                csvWriter.WriteField(row.Website);

                csvWriter.NextRecord();
            }
        });

        public static Stream CreateVenueUploadCsvStream(int rowCount) => CreateVenueUploadCsvStream(csvWriter =>
        {
            foreach (var row in CreateVenueUploadRows(rowCount))
            {
                csvWriter.WriteField(row.ProviderVenueRef);
                csvWriter.WriteField(row.VenueName);
                csvWriter.WriteField(row.AddressLine1);
                csvWriter.WriteField(row.AddressLine2);
                csvWriter.WriteField(row.Town);
                csvWriter.WriteField(row.County);
                csvWriter.WriteField(row.Postcode);
                csvWriter.WriteField(row.Email);
                csvWriter.WriteField(row.Telephone);
                csvWriter.WriteField(row.Website);

                csvWriter.NextRecord();
            }
        });

        public static IEnumerable<CsvVenueRow> CreateVenueUploadRows(int rowCount)
        {
            var venueNames = new HashSet<string>();

            for (int i = 0; i < rowCount; i++)
            {
                // Venue names have to be unique
                string venueName;
                do
                {
                    venueName = Faker.Company.Name();
                }
                while (!venueNames.Add(venueName));

                yield return new CsvVenueRow()
                {
                    ProviderVenueRef = Guid.NewGuid().ToString(),
                    VenueName = venueName,
                    AddressLine1 = Faker.Address.StreetAddress(),
                    AddressLine2 = Faker.Address.SecondaryAddress(),
                    Town = Faker.Address.City(),
                    County = Faker.Address.UkCounty(),
                    Postcode = "AB1 2DE",  // Faker's method sometimes produces invalid postcodes :-/
                    Email = Faker.Internet.Email(),
                    Telephone = "01234 567890",  // There's no Faker method for a UK phone number
                    Website = Faker.Internet.Url()
                };
            }
        }

        public static VenueDataUploadRowInfoCollection ToDataUploadRowCollection(this IEnumerable<CsvVenueRow> rows)
        {
            var rowsArray = rows.ToArray();

            return new VenueDataUploadRowInfoCollection(
                lastRowNumber: rowsArray.Length + 1,
                rows.Select((r, i) => new VenueDataUploadRowInfo(r, rowNumber: i + 2, isSupplementary: false)));
        }
    }
}
