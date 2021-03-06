﻿using System;
using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.FindAnApprenticeshipApi.Helper;
using Dfc.CourseDirectory.FindAnApprenticeshipApi.Models;
using Dfc.CourseDirectory.FindAnApprenticeshipApi.Models.Enums;
using Dfc.CourseDirectory.FindAnApprenticeshipApi.Models.Providers;
using Microsoft.ApplicationInsights.Extensibility;
using Xunit;

namespace Dfc.CourseDirectory.FindAnApprenticeship.Tests.Helper
{
    public class DasHelperTests : IDisposable
    {
        public class DasHelperFixture
        {
            private readonly DASHelper _dasHelper;

            public DasHelperFixture()
            {
                var telemetryClient = MockTelemetryHelper.Initialize();
                _dasHelper = new DASHelper(telemetryClient);
            }

            public DASHelper Sut => _dasHelper;
        }

        public class ConvertToApprenticeshipDeliveryModes : IClassFixture<DasHelperFixture>
        {
            private DASHelper _sut;

            private List<string> _validDeliveryModes = Enum.GetValues(typeof(DeliveryMode))
                    .Cast<DeliveryMode>()
                    .Select(v => v.ToDescription())
                    .ToList();

            public ConvertToApprenticeshipDeliveryModes(DasHelperFixture fixture)
            {
                _sut = fixture.Sut;
            }

            [Theory]
            [JsonFileData("TestData/Location/Locations.json", "Locations")]
            public void AllMatchingModesShouldMapCorrectly(ApprenticeshipLocation location)
            {
                // arrange
                List<string> expected = _validDeliveryModes;

                // act
                var actual = _sut.ConvertToApprenticeshipDeliveryModes(location);

                // assert
                Assert.ProperSubset<string>(expected.ToHashSet(), actual.ToHashSet());
            }
        }

        public class CreateDasProviderFromProvider : IClassFixture<DasHelperFixture>
        {
            private DASHelper _sut;

            public CreateDasProviderFromProvider(DasHelperFixture fixture)
            {
                _sut = fixture.Sut;
            }

            [Theory]
            [JsonFileData("TestData/Provider/Providers.json", "Providers")]
            public void DisplaysTheCorrectPhoneNumber(Provider provider)
            {
                // Arrange
                var contactDetails = provider.ProviderContact.FirstOrDefault();
                var expected =
                    !string.IsNullOrWhiteSpace(contactDetails.ContactTelephone1)
                        ? contactDetails.ContactTelephone1
                        : contactDetails.ContactTelephone2;

                // Act
                var result = _sut.CreateDasProviderFromProvider(123, provider, null);
                var actual = result.Phone;

                // Assert
                Assert.Equal(expected, actual);
            }

        }

        public class ApprenticeshipLocationsToLocations : IClassFixture<DasHelperFixture>
        {
            private DASHelper _sut;

            public ApprenticeshipLocationsToLocations(DasHelperFixture fixture)
            {
                _sut = fixture.Sut;
            }

            [Theory]
            [JsonFileData("TestData/Location/Locations.json", "Locations")]
            public void DisplaysTheCorrectPhoneNumber(ApprenticeshipLocation location)
            {
                // Arrange
                var expected = location.Phone;

                // Act
                var locationsList = new Dictionary<string, ApprenticeshipLocation>() { { "1234", location } };
                var result = _sut.ApprenticeshipLocationsToLocations(1234, locationsList);
                var actual = result.FirstOrDefault().Phone;

                // Assert
                Assert.Equal(expected, actual);
            }

        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
