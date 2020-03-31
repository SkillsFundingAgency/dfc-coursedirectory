using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.LoqateAddressSearch;
using JustEat.HttpClientInterception;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests
{
    public class LoqateAddressSearchServiceTests
    {
        [Fact]
        public async Task SearchByPostcode_ValidRequest_ReturnsParsedResults()
        {
            // Arrange
            var options = new HttpClientInterceptorOptions();

            new HttpRequestInterceptionBuilder()
                .Requests()
                .ForHttps()
                .ForHost("services.postcodeanywhere.co.uk")
                .ForPath("PostcodeAnywhere/Interactive/FindByPostcode/v1.00/json3.ws")
                .IgnoringQuery()
                .Responds()
                .WithJsonContent(new
                {
                    Items = new[]
                    {
                        new { Id = "5702836.00", StreetAddress = "2 Seagrave Road", Place = "Coventry" },
                        new { Id = "5702847.00", StreetAddress = "4 Seagrave Road", Place = "Coventry" },
                        new { Id = "5702859.00", StreetAddress = "6 Seagrave Road", Place = "Coventry" },
                    }
                })
                .RegisterWith(options);

            var httpClient = options.CreateHttpClient();

            var service = new AddressSearchService(httpClient, new Options() { Key = "key" });

            // Act
            var result = await service.SearchByPostcode("CV1 2AA");

            // Assert
            Assert.Equal(3, result.Count);

            Assert.Equal("5702836.00", result.First().Id);
            Assert.Equal("2 Seagrave Road", result.First().StreetAddress);
            Assert.Equal("Coventry", result.First().Place);

            Assert.Equal("5702847.00", result.Skip(1).First().Id);
            Assert.Equal("4 Seagrave Road", result.Skip(1).First().StreetAddress);
            Assert.Equal("Coventry", result.Skip(1).First().Place);

            Assert.Equal("5702859.00", result.Skip(2).First().Id);
            Assert.Equal("6 Seagrave Road", result.Skip(2).First().StreetAddress);
            Assert.Equal("Coventry", result.Skip(2).First().Place);
        }

        [Fact]
        public async Task SearchByPostcode_ErrorResponse_ThrowsLoqateErrorException()
        {
            // Arrange
            var options = new HttpClientInterceptorOptions();

            new HttpRequestInterceptionBuilder()
                .Requests()
                .ForHttps()
                .ForHost("services.postcodeanywhere.co.uk")
                .ForPath("PostcodeAnywhere/Interactive/FindByPostcode/v1.00/json3.ws")
                .IgnoringQuery()
                .Responds()
                .WithJsonContent(new
                {
                    Items = new[]
                    {
                        new
                        {
                            Error = "Error",
                            Description = "Error description",
                            Cause = "Error cause",
                            Resolution = "Error resolution"
                        }
                    }
                })
                .RegisterWith(options);

            var httpClient = options.CreateHttpClient();

            var service = new AddressSearchService(httpClient, new Options() { Key = "key" });

            // Act & Assert
            var ex = await Assert.ThrowsAsync<LoqateErrorException>(() => service.SearchByPostcode("CV1 2AA"));
            Assert.Equal("Error", ex.Error);
            Assert.Equal("Error description", ex.Description);
            Assert.Equal("Error cause", ex.Cause);
            Assert.Equal("Error resolution", ex.Resolution);
        }

        [Fact]
        public async Task GetById_ValidRequest_ReturnsParsedResult()
        {
            // Arrange
            var options = new HttpClientInterceptorOptions();

            new HttpRequestInterceptionBuilder()
                .Requests()
                .ForHttps()
                .ForHost("services.postcodeanywhere.co.uk")
                .ForPath("PostcodeAnywhere/Interactive/RetrieveById/v1.30/json3.ws")
                .IgnoringQuery()
                .Responds()
                .WithJsonContent(new
                {
                    Items = new[]
                    {
                        new
                        {
                            Udprn = 5702847,
                            Company = "",
                            Department = "",
                            Line1 = "4 Seagrave Road",
                            Line2 = "",
                            Line3 = "",
                            Line4 = "",
                            Line5 = "",
                            PostTown = "Coventry",
                            County = "West Midlands",
                            Postcode = "CV1 2AA",
                            Mailsort = 46111,
                            Barcode = "(CV12AA1WM)",
                            Type = "Residential",
                            DeliveryPointSuffix = "1W",
                            SubBuilding = "",
                            BuildingName = "",
                            BuildingNumber = "4",
                            PrimaryStreet = "Seagrave Road",
                            SecondaryStreet = "",
                            DoubleDependentLocality = "",
                            DependentLocality = "",
                            PoBox = "",
                            PrimaryStreetName = "Seagrave",
                            PrimaryStreetType = "Road",
                            SecondaryStreetName = "",
                            SecondaryStreetType = "",
                            CountryName = "England",
                            CountryISO2 = "GB",
                            CountryISO3 = "GBR"
                        }
                    }
                })
                .RegisterWith(options);

            var httpClient = options.CreateHttpClient();

            var service = new AddressSearchService(httpClient, new Options() { Key = "key" });

            // Act
            var result = await service.GetById("123456.0");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("4 Seagrave Road", result.Line1);
            Assert.Equal("", result.Line2);
            Assert.Equal("", result.Line3);
            Assert.Equal("", result.Line4);
            Assert.Equal("Coventry", result.PostTown);
            Assert.Equal("West Midlands", result.County);
            Assert.Equal("CV1 2AA", result.Postcode);
        }

        [Fact]
        public async Task GetById_ItemDoesNotExist_ReturnsNull()
        {
            // Arrange
            var options = new HttpClientInterceptorOptions();

            new HttpRequestInterceptionBuilder()
                .Requests()
                .ForHttps()
                .ForHost("services.postcodeanywhere.co.uk")
                .ForPath("PostcodeAnywhere/Interactive/RetrieveById/v1.30/json3.ws")
                .IgnoringQuery()
                .Responds()
                .WithJsonContent(new
                {
                    Items = new[]
                    {
                        new
                        {
                            Error = "1002",
                            Description = "Id Invalid",
                            Cause = "The Id parameter was not valid.",
                            Resolution = "The Id parameter should be an Id from a Find method. It may contain unusual formatting characters, all of which must be presented."
                        }
                    }
                })
                .RegisterWith(options);

            var httpClient = options.CreateHttpClient();

            var service = new AddressSearchService(httpClient, new Options() { Key = "key" });

            // Act
            var result = await service.GetById("123456.0");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetById_ErrorResponse_ThrowsLoqateErrorException()
        {
            // Arrange
            var options = new HttpClientInterceptorOptions();

            new HttpRequestInterceptionBuilder()
                .Requests()
                .ForHttps()
                .ForHost("services.postcodeanywhere.co.uk")
                .ForPath("PostcodeAnywhere/Interactive/RetrieveById/v1.30/json3.ws")
                .IgnoringQuery()
                .Responds()
                .WithJsonContent(new
                {
                    Items = new[]
                    {
                        new
                        {
                            Error = "Error",
                            Description = "Error description",
                            Cause = "Error cause",
                            Resolution = "Error resolution"
                        }
                    }
                })
                .RegisterWith(options);

            var httpClient = options.CreateHttpClient();

            var service = new AddressSearchService(httpClient, new Options() { Key = "key" });

            // Act & Assert
            var ex = await Assert.ThrowsAsync<LoqateErrorException>(() => service.GetById("123546.0"));
            Assert.Equal("Error", ex.Error);
            Assert.Equal("Error description", ex.Description);
            Assert.Equal("Error cause", ex.Cause);
            Assert.Equal("Error resolution", ex.Resolution);
        }
    }
}
