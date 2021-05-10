using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;

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

        public static Stream CreateVenueUploadCsvStream(int recordCount) => CreateVenueUploadCsvStream(csvWriter =>
        {
            var venueNames = new HashSet<string>();

            for (int i = 0; i < recordCount; i++)
            {
                // Venue names have to be unique
                string venueName;
                do
                {
                    venueName = Faker.Company.Name();
                }
                while (!venueNames.Add(venueName));

                csvWriter.WriteField(Guid.NewGuid().ToString());
                csvWriter.WriteField(venueName);
                csvWriter.WriteField(Faker.Address.StreetAddress());
                csvWriter.WriteField(Faker.Address.SecondaryAddress());
                csvWriter.WriteField(Faker.Address.City());
                csvWriter.WriteField(Faker.Address.UkCounty());
                csvWriter.WriteField(Faker.Address.UkPostCode());
                csvWriter.WriteField(Faker.Internet.Email());
                csvWriter.WriteField(string.Empty); // There's no Faker method for a UK phone number
                csvWriter.WriteField(Faker.Internet.Url());

                csvWriter.NextRecord();
            }
        });
    }
}
