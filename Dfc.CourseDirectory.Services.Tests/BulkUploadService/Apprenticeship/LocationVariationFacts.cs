using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dfc.CourseDirectory.Core.Models;
using Xunit;

namespace Dfc.CourseDirectory.Services.Tests.BulkUploadService.Apprenticeship
{
    public class LocationVariationFacts : TheoryData<LocationVariationFact>
    {
        private const string NoError = null;
        private const string DefaultVenue = ApprenticeshipCsvRowBuilder.DefaultTestVenueName;

        public LocationVariationFacts()
        {
            var facts = new List<LocationVariationFact>
            {
                new LocationVariationFact
                {
                    deliveryMethod = "",
                    deliveryMode = "",
                    acrossEngland = "",
                    nationalDelivery = "",
                    venue = "",
                    region = "",
                    subRegion = "",
                    expectedError = "Validation error on row 2. DELIVERY_METHOD is required.",
                },
                new LocationVariationFact
                {
                    deliveryMethod = "both",
                    deliveryMode = "",
                    acrossEngland = "",
                    nationalDelivery = "",
                    venue = "",
                    region = "",
                    subRegion = "",
                    expectedError = "Validation error on row 2. Venue is missing.",
                },
                new LocationVariationFact
                {
                    deliveryMethod = "both",
                    deliveryMode = "",
                    acrossEngland = "",
                    nationalDelivery = "",
                    venue = DefaultVenue,
                    region = "",
                    subRegion = "",
                    expectedError = "Validation error on row 2. Field DELIVERY_MODE must be a valid Delivery Mode",
                },
                new LocationVariationFact
                {
                    deliveryMethod = "both",
                    deliveryMode = "block",
                    acrossEngland = "",
                    nationalDelivery = "",
                    venue = "",
                    region = "",
                    subRegion = "",
                    expectedError = "Validation error on row 2. Venue is missing.",
                },
                new LocationVariationFact
                {
                    deliveryMethod = "both",
                    deliveryMode = "block",
                    acrossEngland = "",
                    nationalDelivery = "",
                    venue = DefaultVenue,
                    region = "",
                    subRegion = "",
                    expectedError = "Validation error on row 2. Field ACROSS_ENGLAND must contain a value when Delivery Method is 'Both",
                },
                new LocationVariationFact
                {
                    deliveryMethod = "both",
                    deliveryMode = "block",
                    acrossEngland = "junk",
                    nationalDelivery = "",
                    venue = DefaultVenue,
                    region = "",
                    subRegion = "",
                    expectedError = "Validation error on row 2. Field ACROSS_ENGLAND must contain a value when Delivery Method is 'Both",
                },
                new LocationVariationFact
                {
                    deliveryMethod = "both",
                    deliveryMode = "block",
                    acrossEngland = "no",
                    nationalDelivery = "",
                    venue = DefaultVenue,
                    region = "",
                    subRegion = "",
                    expectedError = NoError,
                    expectedOutputDeliveryMode = ApprenticeshipDeliveryMode.BlockRelease,
                },
                new LocationVariationFact
                {
                    deliveryMethod = "both",
                    deliveryMode = "block",
                    acrossEngland = "yes",
                    nationalDelivery = "",
                    venue = DefaultVenue,
                    region = "",
                    subRegion = "",
                    expectedError = NoError,
                    expectedOutputDeliveryMode = ApprenticeshipDeliveryMode.BlockRelease,
                },
                new LocationVariationFact
                {
                    deliveryMethod = "both",
                    deliveryMode = "day",
                    acrossEngland = "",
                    nationalDelivery = "",
                    venue = "",
                    region = "",
                    subRegion = "",
                    expectedError = "Validation error on row 2. Venue is missing.",
                    expectedOutputDeliveryMode = ApprenticeshipDeliveryMode.DayRelease,
                },
                new LocationVariationFact
                {
                    deliveryMethod = "both",
                    deliveryMode = "day",
                    acrossEngland = "",
                    nationalDelivery = "",
                    venue = DefaultVenue,
                    region = "",
                    subRegion = "",
                    expectedError =
                        "Validation error on row 2. Field ACROSS_ENGLAND must contain a value when Delivery Method is 'Both",
                },
                new LocationVariationFact
                {
                    deliveryMethod = "both",
                    deliveryMode = "day",
                    acrossEngland = "junk",
                    nationalDelivery = "",
                    venue = DefaultVenue,
                    region = "",
                    subRegion = "",
                    expectedError =
                        "Validation error on row 2. Field ACROSS_ENGLAND must contain a value when Delivery Method is 'Both",
                },
                new LocationVariationFact
                {
                    deliveryMethod = "both",
                    deliveryMode = "day",
                    acrossEngland = "no",
                    nationalDelivery = "",
                    venue = DefaultVenue,
                    region = "",
                    subRegion = "",
                    expectedError = NoError,
                    expectedOutputDeliveryMode = ApprenticeshipDeliveryMode.DayRelease,
                },
                new LocationVariationFact
                {
                    deliveryMethod = "both",
                    deliveryMode = "day",
                    acrossEngland = "yes",
                    nationalDelivery = "",
                    venue = DefaultVenue,
                    region = "",
                    subRegion = "",
                    expectedError = NoError,
                    expectedOutputDeliveryMode = ApprenticeshipDeliveryMode.DayRelease,
                },
                new LocationVariationFact
                {
                    deliveryMethod = "both",
                    deliveryMode = "employer",
                    acrossEngland = "",
                    nationalDelivery = "",
                    venue = "",
                    region = "",
                    subRegion = "",
                    expectedError = "Validation error on row 2. Venue is missing.",
                    expectedOutputDeliveryMode = ApprenticeshipDeliveryMode.EmployerAddress,
                },
                new LocationVariationFact
                {
                    deliveryMethod = "both",
                    deliveryMode = "employer",
                    acrossEngland = "",
                    nationalDelivery = "",
                    venue = DefaultVenue,
                    region = "",
                    subRegion = "",
                    expectedError =
                        "Validation error on row 2. Field ACROSS_ENGLAND must contain a value when Delivery Method is 'Both",
                    expectedOutputDeliveryMode = ApprenticeshipDeliveryMode.EmployerAddress,
                },
                new LocationVariationFact
                {
                    deliveryMethod = "both",
                    deliveryMode = "employer",
                    acrossEngland = "junk",
                    nationalDelivery = "",
                    venue = DefaultVenue,
                    region = "",
                    subRegion = "",
                    expectedError =
                        "Validation error on row 2. Field ACROSS_ENGLAND must contain a value when Delivery Method is 'Both",
                    expectedOutputDeliveryMode = ApprenticeshipDeliveryMode.EmployerAddress,
                },
                new LocationVariationFact
                {
                    deliveryMethod = "both",
                    deliveryMode = "employer",
                    acrossEngland = "no",
                    nationalDelivery = "",
                    venue = DefaultVenue,
                    region = "",
                    subRegion = "",
                    expectedError = NoError,
                    expectedOutputDeliveryMode = ApprenticeshipDeliveryMode.EmployerAddress,
                },
                new LocationVariationFact
                {
                    deliveryMethod = "both",
                    deliveryMode = "employer",
                    acrossEngland = "yes",
                    nationalDelivery = "",
                    venue = DefaultVenue,
                    region = "",
                    subRegion = "",
                    expectedError = NoError,
                    expectedOutputDeliveryMode = ApprenticeshipDeliveryMode.EmployerAddress,
                },
                new LocationVariationFact
                {
                    deliveryMethod = "both",
                    deliveryMode = "junk",
                    acrossEngland = "",
                    nationalDelivery = "",
                    venue = "",
                    region = "",
                    subRegion = "",
                    expectedError = "Validation error on row 2. Venue is missing.",
                },
                new LocationVariationFact
                {
                    deliveryMethod = "both",
                    deliveryMode = "junk",
                    acrossEngland = "",
                    nationalDelivery = "",
                    venue = DefaultVenue,
                    region = "",
                    subRegion = "",
                    expectedError = "Validation error on row 2. Field DELIVERY_MODE must be a valid Delivery Mode",
                },
                new LocationVariationFact
                {
                    deliveryMethod = "classroom",
                    deliveryMode = "",
                    acrossEngland = "",
                    nationalDelivery = "",
                    venue = "",
                    region = "",
                    subRegion = "",
                    expectedError = "Validation error on row 2. Venue is missing.",
                },
                new LocationVariationFact
                {
                    deliveryMethod = "classroom",
                    deliveryMode = "",
                    acrossEngland = "",
                    nationalDelivery = "",
                    venue = DefaultVenue,
                    region = "",
                    subRegion = "",
                    expectedError = "Validation error on row 2. Field DELIVERY_MODE must be a valid Delivery Mode",
                },
                new LocationVariationFact
                {
                    deliveryMethod = "classroom",
                    deliveryMode = "block",
                    acrossEngland = "",
                    nationalDelivery = "",
                    venue = "",
                    region = "",
                    subRegion = "",
                    expectedError = "Validation error on row 2. Venue is missing.",
                },
                new LocationVariationFact
                {
                    deliveryMethod = "classroom",
                    deliveryMode = "block",
                    acrossEngland = "",
                    nationalDelivery = "",
                    venue = DefaultVenue,
                    region = "",
                    subRegion = "",
                    expectedError = NoError,
                    expectedOutputDeliveryMode = ApprenticeshipDeliveryMode.BlockRelease,
                },
                new LocationVariationFact
                {
                    deliveryMethod = "classroom",
                    deliveryMode = "day",
                    acrossEngland = "",
                    nationalDelivery = "",
                    venue = "",
                    region = "",
                    subRegion = "",
                    expectedError = "Validation error on row 2. Venue is missing.",
                },
                new LocationVariationFact
                {
                    deliveryMethod = "classroom",
                    deliveryMode = "day",
                    acrossEngland = "",
                    nationalDelivery = "",
                    venue = DefaultVenue,
                    region = "",
                    subRegion = "",
                    expectedError = NoError,
                    expectedOutputDeliveryMode = ApprenticeshipDeliveryMode.DayRelease,
                },
                new LocationVariationFact
                {
                    deliveryMethod = "classroom",
                    deliveryMode = "employer",
                    acrossEngland = "",
                    nationalDelivery = "",
                    venue = "",
                    region = "",
                    subRegion = "",
                    expectedError = "Validation error on row 2. Venue is missing.",
                    expectedOutputDeliveryMode = ApprenticeshipDeliveryMode.EmployerAddress,
                },
                new LocationVariationFact
                {
                    deliveryMethod = "classroom",
                    deliveryMode = "employer",
                    acrossEngland = "",
                    nationalDelivery = "",
                    venue = DefaultVenue,
                    region = "",
                    subRegion = "",
                    expectedError = NoError,
                    expectedOutputDeliveryMode = ApprenticeshipDeliveryMode.EmployerAddress,
                },
                new LocationVariationFact
                {
                    deliveryMethod = "employer",
                    deliveryMode = "",
                    acrossEngland = "",
                    nationalDelivery = "",
                    venue = "",
                    region = "",
                    subRegion = "",
                    expectedError =
                        "Validation error on row 2. Field NATIONAL_DELIVERY must contain a value when Delivery Method is 'Employer'",
                },
                new LocationVariationFact
                {
                    deliveryMethod = "employer",
                    deliveryMode = "",
                    acrossEngland = "",
                    nationalDelivery = "",
                    venue = DefaultVenue,
                    region = "",
                    subRegion = "",
                    expectedError =
                        "Validation error on row 2. Field NATIONAL_DELIVERY must contain a value when Delivery Method is 'Employer'",
                },
                new LocationVariationFact
                {
                    deliveryMethod = "employer",
                    deliveryMode = "",
                    acrossEngland = "",
                    nationalDelivery = "junk",
                    venue = DefaultVenue,
                    region = "",
                    subRegion = "",
                    expectedError =
                        "Validation error on row 2. Field NATIONAL_DELIVERY must contain a value when Delivery Method is 'Employer'",
                },
                new LocationVariationFact
                {
                    deliveryMethod = "employer",
                    deliveryMode = "",
                    acrossEngland = "",
                    nationalDelivery = "no",
                    venue = DefaultVenue,
                    region = "",
                    subRegion = "",
                    expectedError = "Validation error on row 2. Fields REGION/SUB_REGION are mandatory",
                },
                new LocationVariationFact
                {
                    deliveryMethod = "employer",
                    deliveryMode = "",
                    acrossEngland = "",
                    nationalDelivery = "no",
                    venue = DefaultVenue,
                    region = "Junk",
                    subRegion = "",
                    expectedError = "Validation error on row 2. Field REGION  contains invalid Region names",
                },
                new LocationVariationFact
                {
                    deliveryMethod = "employer",
                    deliveryMode = "",
                    acrossEngland = "",
                    nationalDelivery = "no",
                    venue = DefaultVenue,
                    region = "South East;Junk",
                    subRegion = "",
                    expectedError = "Validation error on row 2. Field REGION  contains invalid Region names",
                },
                new LocationVariationFact
                {
                    deliveryMethod = "employer",
                    deliveryMode = "",
                    acrossEngland = "",
                    nationalDelivery = "no",
                    venue = DefaultVenue,
                    region = "South East;South West",
                    subRegion = "Devon;Reading;Dorset;Slough",
                    expectedError = NoError,
                    expectedOutputDeliveryMode = null,
                },
                new LocationVariationFact
                {
                    deliveryMethod = "employer",
                    deliveryMode = "",
                    acrossEngland = "",
                    nationalDelivery = "no",
                    venue = DefaultVenue,
                    region = "South West",
                    subRegion = "",
                    expectedError = NoError,
                    expectedOutputDeliveryMode = null,
                },
                new LocationVariationFact
                {
                    deliveryMethod = "employer",
                    deliveryMode = "",
                    acrossEngland = "",
                    nationalDelivery = "no",
                    venue = DefaultVenue,
                    region = "South West",
                    subRegion = "Derby",
                    expectedError = NoError,
                    expectedOutputDeliveryMode = null,
                },
                new LocationVariationFact
                {
                    deliveryMethod = "employer",
                    deliveryMode = "",
                    acrossEngland = "",
                    nationalDelivery = "no",
                    venue = DefaultVenue,
                    region = "South West",
                    subRegion = "Devon",
                    expectedError = NoError,
                    expectedOutputDeliveryMode = null,
                },
                new LocationVariationFact
                {
                    deliveryMethod = "employer",
                    deliveryMode = "",
                    acrossEngland = "",
                    nationalDelivery = "no",
                    venue = DefaultVenue,
                    region = "South West",
                    subRegion = "Devon;Dorset",
                    expectedOutputDeliveryMode = null,
                    expectedError = NoError,
                },
                new LocationVariationFact
                {
                    deliveryMethod = "employer",
                    deliveryMode = "",
                    acrossEngland = "",
                    nationalDelivery = "no",
                    venue = DefaultVenue,
                    region = "South West",
                    subRegion = "Devon;Junk",
                    expectedError = "Validation error on row 2. Field SUB_REGION  contains invalid SubRegion names",
                },
                new LocationVariationFact
                {
                    deliveryMethod = "employer",
                    deliveryMode = "",
                    acrossEngland = "",
                    nationalDelivery = "no",
                    venue = DefaultVenue,
                    region = "South West",
                    subRegion = "Junk",
                    expectedError = "Validation error on row 2. Field SUB_REGION  contains invalid SubRegion names",
                },
                new LocationVariationFact
                {
                    deliveryMethod = "employer",
                    deliveryMode = "",
                    acrossEngland = "",
                    nationalDelivery = "no",
                    venue = DefaultVenue,
                    region = "south west",
                    subRegion = "devon",
                    expectedError = NoError,
                    expectedOutputDeliveryMode = null,
                },
                new LocationVariationFact
                {
                    deliveryMethod = "employer",
                    deliveryMode = "",
                    acrossEngland = "",
                    nationalDelivery = "yes",
                    venue = "",
                    region = "",
                    subRegion = "",
                    expectedError = NoError,
                    expectedOutputDeliveryMode = null,
                },
                new LocationVariationFact
                {
                    deliveryMethod = "employer",
                    deliveryMode = "",
                    acrossEngland = "",
                    nationalDelivery = "yes",
                    venue = DefaultVenue,
                    region = "",
                    subRegion = "",
                    expectedError = NoError,
                    expectedOutputDeliveryMode = null,
                },
                new LocationVariationFact
                {
                    deliveryMethod = "junk",
                    deliveryMode = "",
                    acrossEngland = "",
                    nationalDelivery = "",
                    venue = "",
                    region = "",
                    subRegion = "",
                    expectedError = "Validation error on row 2. Field DELIVERY_METHOD is invalid.",
                },
            };
            foreach (var fact in facts)
            {
                Add(fact);
            }

            /* Test analysis code; uncomment this to generate a csv of all the covered variations which can then be used to check that all desired variations are covered and to facilitate discussions.
                var coverageTable =
                    $"\"{nameof(LocationVariationFact.deliveryMethod)}\",\"{nameof(LocationVariationFact.deliveryMode)}\",\"{nameof(LocationVariationFact.acrossEngland)}\",\"{nameof(LocationVariationFact.nationalDelivery)}\",\"{nameof(LocationVariationFact.venue)}\",\"{nameof(LocationVariationFact.region)}\",\"{nameof(LocationVariationFact.subRegion)}\",\"{nameof(LocationVariationFact.expectedError)}\",\"{nameof(LocationVariationFact.expectedOutputDeliveryMode)}\"{System.Environment.NewLine}"
                    + string.Join(System.Environment.NewLine, facts.Select(f => $"\"{f.deliveryMethod}\",\"{f.deliveryMode}\",\"{f.acrossEngland}\",\"{f.nationalDelivery}\",\"{f.venue}\",\"{f.region}\",\"{f.subRegion}\",\"{f.expectedError}\",\"{f.expectedOutputDeliveryMode}\""))
                    + $"{System.Environment.NewLine}{System.Environment.NewLine}Generated {System.DateTime.Now}";
                System.IO.File.WriteAllText("/tmp/location-variation-coverage.csv", coverageTable);
            */
        }
    }
}
