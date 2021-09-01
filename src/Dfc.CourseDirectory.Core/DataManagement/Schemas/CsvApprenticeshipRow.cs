﻿using System;
using System.Collections.Generic;
using System.Text;
using CsvHelper.Configuration.Attributes;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;

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
        [Index(3), Name("APPRENTICESHIP_WEBPAGE")]
        public string ApprenticeshipWebpage { get; set; }
        [Index(4), Name("CONTACT_EMAIL")]
        public string ContactEmail { get; set; }
        [Index(5), Name("CONTACT_PHONE")]
        public string ContactPhone { get; set; }
        [Index(6), Name("CONTACT_URL")]
        public string ContactUrl { get; set; }
        [Index(7), Name("DELIVERY_METHOD")]
        public string DeliveryMethod { get; set; }
        [Index(8), Name("VENUE")]
        public string VenueName { get; set; }
        [Index(9), Name("YOUR_VENUE_REFERENCE")]
        public string YourVenueReference { get; set; }
        [Index(10), Name("RADIUS")]
        public string Radius { get; set; }
        [Index(11), Name("DELIVERY_MODE")]
        public string DeliveryMode { get; set; }
        [Index(12), Name("NATIONAL_DELIVERY")]
        public string NationalDelivery { get; set; }
        [Index(13), Name("SUB_REGION")]
        public string SubRegion { get; set; }

        public static CsvApprenticeshipRow FromModel(ApprenticeshipUploadRow row) => new CsvApprenticeshipRow()
        {
            //StandardCode = row.StandardCode,
            //StandardVersion = row.StandardVersion,
            ApprenticeshipInformation = row.ApprenticeshipInformation,
            ApprenticeshipWebpage = row.ApprenticeshipWebpage,
            ContactEmail = row.ContactEmail,
            ContactPhone = row.ContactPhone,
            ContactUrl = row.ContactUrl,
            DeliveryMethod = row.DeliveryMethod,
            VenueName = row.VenueName,
            YourVenueReference = row.YourVenueReference,
            Radius = row.Radius,
            DeliveryMode = row.DeliveryMode,
            NationalDelivery = row.NationalDelivery,
            SubRegion = row.SubRegions
        };
    }
}
