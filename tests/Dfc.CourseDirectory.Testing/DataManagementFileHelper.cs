﻿using System;
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
        public static Stream CreateCsvStream(Action<CsvWriter> writeRows)
        {
            var stream = new MemoryStream();

            using (var streamWriter = new StreamWriter(stream, leaveOpen: true))
            using (var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
            {
                writeRows(csvWriter);
            }

            stream.Seek(0L, SeekOrigin.Begin);

            return stream;
        }

        public static Stream CreateCourseUploadCsvStream(Action<CsvWriter> writeRows) =>
            CreateCsvStream(csvWriter =>
            {
                // N.B. We deliberately do not use the CsvCourseRow class here to ensure we notice if any columns change name

                csvWriter.WriteField("LARS_QAN");
                csvWriter.WriteField("WHO_THIS_COURSE_IS_FOR");
                csvWriter.WriteField("ENTRY_REQUIREMENTS");
                csvWriter.WriteField("WHAT_YOU_WILL_LEARN");
                csvWriter.WriteField("HOW_YOU_WILL_LEARN");
                csvWriter.WriteField("WHAT_YOU_WILL_NEED_TO_BRING");
                csvWriter.WriteField("HOW_YOU_WILL_BE_ASSESSED");
                csvWriter.WriteField("WHERE_NEXT");
                csvWriter.WriteField("COURSE_NAME");
                csvWriter.WriteField("YOUR_REFERENCE");
                csvWriter.WriteField("DELIVERY_MODE");
                csvWriter.WriteField("START_DATE");
                csvWriter.WriteField("FLEXIBLE_START_DATE");
                csvWriter.WriteField("VENUE_NAME");
                csvWriter.WriteField("YOUR_VENUE_REFERENCE");
                csvWriter.WriteField("NATIONAL_DELIVERY");
                csvWriter.WriteField("SUB_REGION");
                csvWriter.WriteField("COURSE_WEBPAGE");
                csvWriter.WriteField("COST");
                csvWriter.WriteField("COST_DESCRIPTION");
                csvWriter.WriteField("DURATION");
                csvWriter.WriteField("DURATION_UNIT");
                csvWriter.WriteField("STUDY_MODE");
                csvWriter.WriteField("ATTENDANCE_PATTERN");
                csvWriter.NextRecord();

                writeRows(csvWriter);
            });

        public static Stream CreateCourseUploadCsvStream(params CsvCourseRow[] rows) => CreateCourseUploadCsvStream(csvWriter =>
        {
            foreach (var row in rows)
            {
                csvWriter.WriteField(row.LearnAimRef);
                csvWriter.WriteField(row.WhoThisCourseIsFor);
                csvWriter.WriteField(row.EntryRequirements);
                csvWriter.WriteField(row.WhatYouWillLearn);
                csvWriter.WriteField(row.HowYouWillLearn);
                csvWriter.WriteField(row.WhatYouWillNeedToBring);
                csvWriter.WriteField(row.HowYouWillBeAssessed);
                csvWriter.WriteField(row.WhereNext);
                csvWriter.WriteField(row.CourseName);
                csvWriter.WriteField(row.ProviderCourseRef);
                csvWriter.WriteField(row.DeliveryMode);
                csvWriter.WriteField(row.StartDate);
                csvWriter.WriteField(row.FlexibleStartDate);
                csvWriter.WriteField(row.VenueName);
                csvWriter.WriteField(row.ProviderVenueRef);
                csvWriter.WriteField(row.NationalDelivery);
                csvWriter.WriteField(row.SubRegions);
                csvWriter.WriteField(row.CourseWebPage);
                csvWriter.WriteField(row.Cost);
                csvWriter.WriteField(row.CostDescription);
                csvWriter.WriteField(row.Duration);
                csvWriter.WriteField(row.DurationUnit);
                csvWriter.WriteField(row.StudyMode);
                csvWriter.WriteField(row.AttendancePattern);

                csvWriter.NextRecord();
            }
        });

        public static Stream CreateCourseUploadCsvStream(string learningAimRef, int rowCount) =>
            CreateCourseUploadCsvStream(CreateCourseUploadRows(learningAimRef, rowCount).ToArray());

        public static IEnumerable<CsvCourseRow> CreateCourseUploadRows(string learningAimRef, int rowCount)
        {
            for (int i = 0; i < rowCount; i++)
            {
                yield return new CsvCourseRow()
                {
                    LearnAimRef = learningAimRef,
                    WhoThisCourseIsFor = "Who this course is for",
                    EntryRequirements = "",
                    WhatYouWillLearn = "",
                    HowYouWillLearn = "",
                    WhatYouWillNeedToBring = "",
                    HowYouWillBeAssessed = "",
                    WhereNext = "",
                    CourseName = "Course name",
                    ProviderCourseRef = "",
                    DeliveryMode = "Online",
                    StartDate = "",
                    FlexibleStartDate = "yes",
                    VenueName = "",
                    ProviderVenueRef = "",
                    NationalDelivery = "",
                    SubRegions = "",
                    CourseWebPage = "",
                    Cost = "",
                    CostDescription = "Free",
                    Duration = "2",
                    DurationUnit = "years",
                    StudyMode = "",
                    AttendancePattern = ""
                };
            }
        }

        public static Stream CreateVenueUploadCsvStream(Action<CsvWriter> writeRows) =>
            CreateCsvStream(csvWriter =>
            {
                // N.B. We deliberately do not use the VenueRow class here to ensure we notice if any columns change name

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

                writeRows(csvWriter);
            });

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

        public static Stream CreateVenueUploadCsvStream(int rowCount) =>
            CreateVenueUploadCsvStream(CreateVenueUploadRows(rowCount).ToArray());

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
                    AddressLine1 = FakerEx.StreetAddressSafe(),
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

        public static CourseDataUploadRowInfoCollection ToDataUploadRowCollection(this IEnumerable<CsvCourseRow> rows)
        {
            var rowInfos = new List<CourseDataUploadRowInfo>();

            foreach (var group in CsvCourseRow.GroupRows(rows))
            {
                var courseId = Guid.NewGuid();

                foreach (var row in group)
                {
                    rowInfos.Add(new CourseDataUploadRowInfo(row, rowNumber: rowInfos.Count + 2, courseId));
                }
            }

            return new CourseDataUploadRowInfoCollection(rowInfos);
        }

        public static VenueDataUploadRowInfoCollection ToDataUploadRowCollection(this IEnumerable<CsvVenueRow> rows)
        {
            var rowsArray = rows.ToArray();

            return new VenueDataUploadRowInfoCollection(
                lastRowNumber: rowsArray.Length + 1,
                rows.Select((r, i) => new VenueDataUploadRowInfo(r, rowNumber: i + 2, isSupplementary: false)));
        }

        public static Stream CreateApprenticeshipUploadCsvStream(Action<CsvWriter> writeRows) => CreateCsvStream(csvWriter =>
        {
            csvWriter.WriteField("STANDARD_CODE");
            csvWriter.WriteField("STANDARD_VERSION");
            csvWriter.WriteField("APPRENTICESHIP_INFORMATION");
            csvWriter.WriteField("APPRENTICESHIP_WEBPAGE");
            csvWriter.WriteField("CONTACT_EMAIL");
            csvWriter.WriteField("CONTACT_PHONE");
            csvWriter.WriteField("CONTACT_URL");
            csvWriter.WriteField("DELIVERY_METHOD");
            csvWriter.WriteField("VENUE");
            csvWriter.WriteField("YOUR_VENUE_REFERENCE");
            csvWriter.WriteField("RADIUS");
            csvWriter.WriteField("DELIVERY_MODE");
            csvWriter.WriteField("NATIONAL_DELIVERY");
            csvWriter.WriteField("SUB_REGION");
            csvWriter.NextRecord();
            writeRows(csvWriter);
        });

        public static Stream CreateApprenticeshipUploadCsvStream(params CsvApprenticeshipRow[] rows) => CreateCourseUploadCsvStream(csvWriter =>
        {
            foreach (var row in rows)
            {
                csvWriter.WriteField(row.StandardCode);
                csvWriter.WriteField(row.StandardVersion);
                csvWriter.WriteField(row.ApprenticeshipInformation);
                csvWriter.WriteField(row.ApprenticeshipWebpage);
                csvWriter.WriteField(row.ContactEmail);
                csvWriter.WriteField(row.ContactPhone);
                csvWriter.WriteField(row.ContactUrl);
                csvWriter.WriteField(row.DeliveryMethod);
                csvWriter.WriteField(row.Venue);
                csvWriter.WriteField(row.YourVenueReference);
                csvWriter.WriteField(row.Radius);
                csvWriter.WriteField(row.DeliveryMode);
                csvWriter.WriteField(row.NationalDelivery);
                csvWriter.WriteField(row.SubRegion);
                csvWriter.NextRecord();
            }
        });

        public static Stream CreateApprenticeshipUploadCsvStream(int rowCount) => CreateApprenticeshipUploadCsvStream(CreateApprenticeshipUploadRows(rowCount).ToArray());

        public static IEnumerable<CsvApprenticeshipRow> CreateApprenticeshipUploadRows(int rowCount)
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

                yield return new CsvApprenticeshipRow()
                {
                    Venue = venueName,
                    StandardVersion = "1",
                    StandardCode = "1",
                    ApprenticeshipInformation = "",
                    ApprenticeshipWebpage = "",
                    ContactEmail = "",
                    ContactPhone = "",
                    ContactUrl = "",
                    DeliveryMethod = "",
                    YourVenueReference = "",
                    Radius = "",
                    DeliveryMode = "",
                    NationalDelivery = "",
                    SubRegion = ""
                };
            }
        }

        public static ApprenticeshipDataUploadRowInfoCollection ToDataUploadRowCollection(this IEnumerable<CsvApprenticeshipRow> rows)
        {
            var rowInfos = new List<ApprenticeshipDataUploadRowInfo>();

            foreach (var row in rows)
            {
                var courseId = Guid.NewGuid();
                rowInfos.Add(new ApprenticeshipDataUploadRowInfo(row, rowNumber: rowInfos.Count + 2, courseId));
            }
            return new ApprenticeshipDataUploadRowInfoCollection(rowInfos);
        }
    }
}
