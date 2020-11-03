using System.Collections.Generic;
using Dfc.CourseDirectory.Core.Models;
using Xunit;

namespace Dfc.CourseDirectory.Services.Tests.BulkUploadService
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
                    DeliveryMethod = "",
                    DeliveryMode = "",
                    AcrossEngland = "",
                    NationalDelivery = "",
                    Venue = "",
                    Region = "",
                    SubRegion = "",
                    ExpectedError = "Validation error on row 2. DELIVERY_METHOD is required.",
                },
                new LocationVariationFact
                {
                    DeliveryMethod = "both",
                    DeliveryMode = "",
                    AcrossEngland = "",
                    NationalDelivery = "",
                    Venue = "",
                    Region = "",
                    SubRegion = "",
                    ExpectedError = "Validation error on row 2. Venue is missing.",
                },
                new LocationVariationFact
                {
                    DeliveryMethod = "both",
                    DeliveryMode = "",
                    AcrossEngland = "",
                    NationalDelivery = "",
                    Venue = DefaultVenue,
                    Region = "",
                    SubRegion = "",
                    ExpectedError = "Validation error on row 2. Field DELIVERY_MODE must be a valid Delivery Mode",
                },
                new LocationVariationFact
                {
                    DeliveryMethod = "both",
                    DeliveryMode = "block",
                    AcrossEngland = "",
                    NationalDelivery = "",
                    Venue = "",
                    Region = "",
                    SubRegion = "",
                    ExpectedError = "Validation error on row 2. Venue is missing.",
                },
                new LocationVariationFact
                {
                    DeliveryMethod = "both",
                    DeliveryMode = "block",
                    AcrossEngland = "",
                    NationalDelivery = "",
                    Venue = DefaultVenue,
                    Region = "",
                    SubRegion = "",
                    ExpectedError = "Validation error on row 2. Field ACROSS_ENGLAND must contain a value when Delivery Method is 'Both",
                },
                new LocationVariationFact
                {
                    DeliveryMethod = "both",
                    DeliveryMode = "block",
                    AcrossEngland = "junk",
                    NationalDelivery = "",
                    Venue = DefaultVenue,
                    Region = "",
                    SubRegion = "",
                    ExpectedError = "Validation error on row 2. Field ACROSS_ENGLAND must contain a value when Delivery Method is 'Both",
                },
                new LocationVariationFact
                {
                    DeliveryMethod = "both",
                    DeliveryMode = "block",
                    AcrossEngland = "no",
                    NationalDelivery = "",
                    Venue = DefaultVenue,
                    Region = "",
                    SubRegion = "",
                    ExpectedError = NoError,
                    ExpectedOutputDeliveryMode = ApprenticeshipDeliveryMode.BlockRelease,
                },
                new LocationVariationFact
                {
                    DeliveryMethod = "both",
                    DeliveryMode = "block",
                    AcrossEngland = "yes",
                    NationalDelivery = "",
                    Venue = DefaultVenue,
                    Region = "",
                    SubRegion = "",
                    ExpectedError = NoError,
                    ExpectedOutputDeliveryMode = ApprenticeshipDeliveryMode.BlockRelease,
                },
                new LocationVariationFact
                {
                    DeliveryMethod = "both",
                    DeliveryMode = "day",
                    AcrossEngland = "",
                    NationalDelivery = "",
                    Venue = "",
                    Region = "",
                    SubRegion = "",
                    ExpectedError = "Validation error on row 2. Venue is missing.",
                    ExpectedOutputDeliveryMode = ApprenticeshipDeliveryMode.DayRelease,
                },
                new LocationVariationFact
                {
                    DeliveryMethod = "both",
                    DeliveryMode = "day",
                    AcrossEngland = "",
                    NationalDelivery = "",
                    Venue = DefaultVenue,
                    Region = "",
                    SubRegion = "",
                    ExpectedError =
                        "Validation error on row 2. Field ACROSS_ENGLAND must contain a value when Delivery Method is 'Both",
                },
                new LocationVariationFact
                {
                    DeliveryMethod = "both",
                    DeliveryMode = "day",
                    AcrossEngland = "junk",
                    NationalDelivery = "",
                    Venue = DefaultVenue,
                    Region = "",
                    SubRegion = "",
                    ExpectedError =
                        "Validation error on row 2. Field ACROSS_ENGLAND must contain a value when Delivery Method is 'Both",
                },
                new LocationVariationFact
                {
                    DeliveryMethod = "both",
                    DeliveryMode = "day",
                    AcrossEngland = "no",
                    NationalDelivery = "",
                    Venue = DefaultVenue,
                    Region = "",
                    SubRegion = "",
                    ExpectedError = NoError,
                    ExpectedOutputDeliveryMode = ApprenticeshipDeliveryMode.DayRelease,
                },
                new LocationVariationFact
                {
                    DeliveryMethod = "both",
                    DeliveryMode = "day",
                    AcrossEngland = "yes",
                    NationalDelivery = "",
                    Venue = DefaultVenue,
                    Region = "",
                    SubRegion = "",
                    ExpectedError = NoError,
                    ExpectedOutputDeliveryMode = ApprenticeshipDeliveryMode.DayRelease,
                },
                new LocationVariationFact
                {
                    DeliveryMethod = "both",
                    DeliveryMode = "employer",
                    AcrossEngland = "",
                    NationalDelivery = "",
                    Venue = "",
                    Region = "",
                    SubRegion = "",
                    ExpectedError = "Validation error on row 2. Venue is missing.",
                    ExpectedOutputDeliveryMode = ApprenticeshipDeliveryMode.EmployerAddress,
                },
                new LocationVariationFact
                {
                    DeliveryMethod = "both",
                    DeliveryMode = "employer",
                    AcrossEngland = "",
                    NationalDelivery = "",
                    Venue = DefaultVenue,
                    Region = "",
                    SubRegion = "",
                    ExpectedError =
                        "Validation error on row 2. Field ACROSS_ENGLAND must contain a value when Delivery Method is 'Both",
                    ExpectedOutputDeliveryMode = ApprenticeshipDeliveryMode.EmployerAddress,
                },
                new LocationVariationFact
                {
                    DeliveryMethod = "both",
                    DeliveryMode = "employer",
                    AcrossEngland = "junk",
                    NationalDelivery = "",
                    Venue = DefaultVenue,
                    Region = "",
                    SubRegion = "",
                    ExpectedError =
                        "Validation error on row 2. Field ACROSS_ENGLAND must contain a value when Delivery Method is 'Both",
                    ExpectedOutputDeliveryMode = ApprenticeshipDeliveryMode.EmployerAddress,
                },
                new LocationVariationFact
                {
                    DeliveryMethod = "both",
                    DeliveryMode = "employer",
                    AcrossEngland = "no",
                    NationalDelivery = "",
                    Venue = DefaultVenue,
                    Region = "",
                    SubRegion = "",
                    ExpectedError = NoError,
                    ExpectedOutputDeliveryMode = ApprenticeshipDeliveryMode.EmployerAddress,
                },
                new LocationVariationFact
                {
                    DeliveryMethod = "both",
                    DeliveryMode = "employer",
                    AcrossEngland = "yes",
                    NationalDelivery = "",
                    Venue = DefaultVenue,
                    Region = "",
                    SubRegion = "",
                    ExpectedError = NoError,
                    ExpectedOutputDeliveryMode = ApprenticeshipDeliveryMode.EmployerAddress,
                },
                new LocationVariationFact
                {
                    DeliveryMethod = "both",
                    DeliveryMode = "junk",
                    AcrossEngland = "",
                    NationalDelivery = "",
                    Venue = "",
                    Region = "",
                    SubRegion = "",
                    ExpectedError = "Validation error on row 2. Venue is missing.",
                },
                new LocationVariationFact
                {
                    DeliveryMethod = "both",
                    DeliveryMode = "junk",
                    AcrossEngland = "",
                    NationalDelivery = "",
                    Venue = DefaultVenue,
                    Region = "",
                    SubRegion = "",
                    ExpectedError = "Validation error on row 2. Field DELIVERY_MODE must be a valid Delivery Mode",
                },
                new LocationVariationFact
                {
                    DeliveryMethod = "classroom",
                    DeliveryMode = "",
                    AcrossEngland = "",
                    NationalDelivery = "",
                    Venue = "",
                    Region = "",
                    SubRegion = "",
                    ExpectedError = "Validation error on row 2. Venue is missing.",
                },
                new LocationVariationFact
                {
                    DeliveryMethod = "classroom",
                    DeliveryMode = "",
                    AcrossEngland = "",
                    NationalDelivery = "",
                    Venue = DefaultVenue,
                    Region = "",
                    SubRegion = "",
                    ExpectedError = "Validation error on row 2. Field DELIVERY_MODE must be a valid Delivery Mode",
                },
                new LocationVariationFact
                {
                    DeliveryMethod = "classroom",
                    DeliveryMode = "block",
                    AcrossEngland = "",
                    NationalDelivery = "",
                    Venue = "",
                    Region = "",
                    SubRegion = "",
                    ExpectedError = "Validation error on row 2. Venue is missing.",
                },
                new LocationVariationFact
                {
                    DeliveryMethod = "classroom",
                    DeliveryMode = "block",
                    AcrossEngland = "",
                    NationalDelivery = "",
                    Venue = DefaultVenue,
                    Region = "",
                    SubRegion = "",
                    ExpectedError = NoError,
                    ExpectedOutputDeliveryMode = ApprenticeshipDeliveryMode.BlockRelease,
                },
                new LocationVariationFact
                {
                    DeliveryMethod = "classroom",
                    DeliveryMode = "day",
                    AcrossEngland = "",
                    NationalDelivery = "",
                    Venue = "",
                    Region = "",
                    SubRegion = "",
                    ExpectedError = "Validation error on row 2. Venue is missing.",
                },
                new LocationVariationFact
                {
                    DeliveryMethod = "classroom",
                    DeliveryMode = "day",
                    AcrossEngland = "",
                    NationalDelivery = "",
                    Venue = DefaultVenue,
                    Region = "",
                    SubRegion = "",
                    ExpectedError = NoError,
                    ExpectedOutputDeliveryMode = ApprenticeshipDeliveryMode.DayRelease,
                },
                new LocationVariationFact
                {
                    DeliveryMethod = "classroom",
                    DeliveryMode = "employer",
                    AcrossEngland = "",
                    NationalDelivery = "",
                    Venue = "",
                    Region = "",
                    SubRegion = "",
                    ExpectedError = "Validation error on row 2. Venue is missing.",
                    ExpectedOutputDeliveryMode = ApprenticeshipDeliveryMode.EmployerAddress,
                },
                new LocationVariationFact
                {
                    DeliveryMethod = "classroom",
                    DeliveryMode = "employer",
                    AcrossEngland = "",
                    NationalDelivery = "",
                    Venue = DefaultVenue,
                    Region = "",
                    SubRegion = "",
                    ExpectedError = NoError,
                    ExpectedOutputDeliveryMode = ApprenticeshipDeliveryMode.EmployerAddress,
                },
                new LocationVariationFact
                {
                    DeliveryMethod = "employer",
                    DeliveryMode = "",
                    AcrossEngland = "",
                    NationalDelivery = "",
                    Venue = "",
                    Region = "",
                    SubRegion = "",
                    ExpectedError =
                        "Validation error on row 2. Field NATIONAL_DELIVERY must contain a value when Delivery Method is 'Employer'",
                    ExpectedOutputDeliveryMode = ApprenticeshipDeliveryMode.EmployerAddress,
                },
                new LocationVariationFact
                {
                    DeliveryMethod = "employer",
                    DeliveryMode = "",
                    AcrossEngland = "",
                    NationalDelivery = "",
                    Venue = DefaultVenue,
                    Region = "",
                    SubRegion = "",
                    ExpectedError =
                        "Validation error on row 2. Field NATIONAL_DELIVERY must contain a value when Delivery Method is 'Employer'",
                    ExpectedOutputDeliveryMode = ApprenticeshipDeliveryMode.EmployerAddress,
                },
                new LocationVariationFact
                {
                    DeliveryMethod = "employer",
                    DeliveryMode = "",
                    AcrossEngland = "",
                    NationalDelivery = "junk",
                    Venue = DefaultVenue,
                    Region = "",
                    SubRegion = "",
                    ExpectedError =
                        "Validation error on row 2. Field NATIONAL_DELIVERY must contain a value when Delivery Method is 'Employer'",
                    ExpectedOutputDeliveryMode = ApprenticeshipDeliveryMode.EmployerAddress,
                },
                new LocationVariationFact
                {
                    DeliveryMethod = "employer",
                    DeliveryMode = "",
                    AcrossEngland = "",
                    NationalDelivery = "no",
                    Venue = DefaultVenue,
                    Region = "",
                    SubRegion = "",
                    ExpectedError = "Validation error on row 2. Fields REGION/SUB_REGION are mandatory",
                    ExpectedOutputDeliveryMode = ApprenticeshipDeliveryMode.EmployerAddress,
                },
                new LocationVariationFact
                {
                    DeliveryMethod = "employer",
                    DeliveryMode = "",
                    AcrossEngland = "",
                    NationalDelivery = "no",
                    Venue = DefaultVenue,
                    Region = "Junk",
                    SubRegion = "",
                    ExpectedError = "Validation error on row 2. Field REGION  contains invalid Region names",
                    ExpectedOutputDeliveryMode = ApprenticeshipDeliveryMode.EmployerAddress,
                },
                new LocationVariationFact
                {
                    DeliveryMethod = "employer",
                    DeliveryMode = "",
                    AcrossEngland = "",
                    NationalDelivery = "no",
                    Venue = DefaultVenue,
                    Region = "South East;Junk",
                    SubRegion = "",
                    ExpectedError = "Validation error on row 2. Field REGION  contains invalid Region names",
                    ExpectedOutputDeliveryMode = ApprenticeshipDeliveryMode.EmployerAddress,
                },
                new LocationVariationFact
                {
                    DeliveryMethod = "employer",
                    DeliveryMode = "",
                    AcrossEngland = "",
                    NationalDelivery = "no",
                    Venue = DefaultVenue,
                    Region = "South East;South West",
                    SubRegion = "Devon;Reading;Dorset;Slough",
                    ExpectedError = NoError,
                    ExpectedOutputDeliveryMode = ApprenticeshipDeliveryMode.EmployerAddress,
                },
                new LocationVariationFact
                {
                    DeliveryMethod = "employer",
                    DeliveryMode = "",
                    AcrossEngland = "",
                    NationalDelivery = "no",
                    Venue = DefaultVenue,
                    Region = "South West",
                    SubRegion = "",
                    ExpectedError = NoError,
                    ExpectedOutputDeliveryMode = ApprenticeshipDeliveryMode.EmployerAddress,
                },
                new LocationVariationFact
                {
                    DeliveryMethod = "employer",
                    DeliveryMode = "",
                    AcrossEngland = "",
                    NationalDelivery = "no",
                    Venue = DefaultVenue,
                    Region = "South West",
                    SubRegion = "Derby",
                    ExpectedError = NoError,
                    ExpectedOutputDeliveryMode = ApprenticeshipDeliveryMode.EmployerAddress,
                },
                new LocationVariationFact
                {
                    DeliveryMethod = "employer",
                    DeliveryMode = "",
                    AcrossEngland = "",
                    NationalDelivery = "no",
                    Venue = DefaultVenue,
                    Region = "South West",
                    SubRegion = "Devon",
                    ExpectedError = NoError,
                    ExpectedOutputDeliveryMode = ApprenticeshipDeliveryMode.EmployerAddress,
                },
                new LocationVariationFact
                {
                    DeliveryMethod = "employer",
                    DeliveryMode = "",
                    AcrossEngland = "",
                    NationalDelivery = "no",
                    Venue = DefaultVenue,
                    Region = "South West",
                    SubRegion = "Devon;Dorset",
                    ExpectedError = NoError,
                    ExpectedOutputDeliveryMode = ApprenticeshipDeliveryMode.EmployerAddress,
                },
                new LocationVariationFact
                {
                    DeliveryMethod = "employer",
                    DeliveryMode = "",
                    AcrossEngland = "",
                    NationalDelivery = "no",
                    Venue = DefaultVenue,
                    Region = "South West",
                    SubRegion = "Devon;Junk",
                    ExpectedError = "Validation error on row 2. Field SUB_REGION  contains invalid SubRegion names",
                },
                new LocationVariationFact
                {
                    DeliveryMethod = "employer",
                    DeliveryMode = "",
                    AcrossEngland = "",
                    NationalDelivery = "no",
                    Venue = DefaultVenue,
                    Region = "South West",
                    SubRegion = "Junk",
                    ExpectedError = "Validation error on row 2. Field SUB_REGION  contains invalid SubRegion names",
                },
                new LocationVariationFact
                {
                    DeliveryMethod = "employer",
                    DeliveryMode = "",
                    AcrossEngland = "",
                    NationalDelivery = "no",
                    Venue = DefaultVenue,
                    Region = "south west",
                    SubRegion = "devon",
                    ExpectedError = NoError,
                    ExpectedOutputDeliveryMode = ApprenticeshipDeliveryMode.EmployerAddress,
                },
                new LocationVariationFact
                {
                    DeliveryMethod = "employer",
                    DeliveryMode = "",
                    AcrossEngland = "",
                    NationalDelivery = "yes",
                    Venue = "",
                    Region = "",
                    SubRegion = "",
                    ExpectedError = NoError,
                    ExpectedOutputDeliveryMode = ApprenticeshipDeliveryMode.EmployerAddress,
                },
                new LocationVariationFact
                {
                    DeliveryMethod = "employer",
                    DeliveryMode = "",
                    AcrossEngland = "",
                    NationalDelivery = "yes",
                    Venue = DefaultVenue,
                    Region = "",
                    SubRegion = "",
                    ExpectedError = NoError,
                    ExpectedOutputDeliveryMode = ApprenticeshipDeliveryMode.EmployerAddress,
                },
                new LocationVariationFact
                {
                    DeliveryMethod = "junk",
                    DeliveryMode = "",
                    AcrossEngland = "",
                    NationalDelivery = "",
                    Venue = "",
                    Region = "",
                    SubRegion = "",
                    ExpectedError = "Validation error on row 2. Field DELIVERY_METHOD is invalid.",
                },
            };
            foreach (var fact in facts)
            {
                Add(fact);
            }

            /* Test analysis code; uncomment this to generate a csv of all the covered variations which can then be used to check that all desired variations are covered and to facilitate discussions.
                var coverageTable =
                    $"\"{nameof(LocationVariationFact.DeliveryMethod)}\",\"{nameof(LocationVariationFact.DeliveryMode)}\",\"{nameof(LocationVariationFact.AcrossEngland)}\",\"{nameof(LocationVariationFact.NationalDelivery)}\",\"{nameof(LocationVariationFact.Venue)}\",\"{nameof(LocationVariationFact.Region)}\",\"{nameof(LocationVariationFact.SubRegion)}\",\"{nameof(LocationVariationFact.ExpectedError)}\",\"{nameof(LocationVariationFact.ExpectedOutputDeliveryMode)}\"{System.Environment.NewLine}"
                    + string.Join(System.Environment.NewLine, facts.Select(f => $"\"{f.DeliveryMethod}\",\"{f.DeliveryMode}\",\"{f.AcrossEngland}\",\"{f.NationalDelivery}\",\"{f.Venue}\",\"{f.Region}\",\"{f.SubRegion}\",\"{f.ExpectedError}\",\"{f.ExpectedOutputDeliveryMode}\""))
                    + $"{System.Environment.NewLine}{System.Environment.NewLine}Generated {System.DateTime.Now}";
                var coverageFileName = Path.GetTempFileName();
                System.IO.File.WriteAllText(coverageFileName, coverageTable);
                Console.Out.WriteLine($"Coverage csv exported to {coverageFileName}");
            */
        }
    }
}
