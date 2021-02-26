using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.AddressSearch;
using JustEat.HttpClientInterception;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.AddressSearch
{
    public class GetAddressAddressSearchServiceTests
    {
        [Fact]
        public async Task SearchByPostcode_WithValidRequest_ReturnsParsedResults()
        {
            // Arrange
            var httpRequestInterceptor = new HttpClientInterceptorOptions();

            new HttpRequestInterceptionBuilder()
                .Requests()
                .ForHttps()
                .ForHost("example.com")
                .ForPath("getaddress/XX2 00X")
                .ForQuery("api-key=key&expand=true&sort=true")
                .Responds()
                .WithJsonContent(new
                {
                    postcode = "XX2 00X",
                    latitude = 51.39020538330078,
                    longitude = -0.1320359706878662,
                    addresses = new[]
                    {
                        new
                        {
                            formatted_address = new[] { "658 Mitcham Road", "", "", "Croydon", "Surrey" },
                            thoroughfare = "Mitcham Road",
                            building_name = "",
                            sub_building_name = "",
                            sub_building_number = "",
                            building_number = "658",
                            line_1 = "658 Mitcham Road",
                            line_2 = "",
                            line_3 = "",
                            line_4 = "",
                            locality = "",
                            town_or_city = "Croydon",
                            county = "Surrey",
                            district = "Croydon",
                            country = "England"
                        },
                        new
                        {
                            formatted_address = new[] { "660 Mitcham Road", "", "", "Croydon", "Surrey" },
                            thoroughfare = "Mitcham Road",
                            building_name = "",
                            sub_building_name = "",
                            sub_building_number = "",
                            building_number = "660",
                            line_1 = "660 Mitcham Road",
                            line_2 = "",
                            line_3 = "",
                            line_4 = "",
                            locality = "",
                            town_or_city = "Croydon",
                            county = "Surrey",
                            district = "Croydon",
                            country = "England"
                        },
                        new
                        {
                            formatted_address = new[] { "Lanfranc School House", "Mitcham Road", "", "Croydon", "Surrey" },
                            thoroughfare = "Mitcham Road",
                            building_name = "",
                            sub_building_name = "Lanfranc School House",
                            sub_building_number = "",
                            building_number = "",
                            line_1 = "Lanfranc School House",
                            line_2 = "Mitcham Road",
                            line_3 = "",
                            line_4 = "",
                            locality = "",
                            town_or_city = "Croydon",
                            county = "Surrey",
                            district = "Croydon",
                            country = "England"
                        }
                    }
                })
                .RegisterWith(httpRequestInterceptor);

            var httpClient = httpRequestInterceptor.CreateHttpClient();

            var options = new Mock<IOptions<GetAddressAddressSearchServiceOptions>>();
            options.Setup(s => s.Value).Returns(new GetAddressAddressSearchServiceOptions { ApiUrl = "https://example.com/getaddress/{0}", ApiKey = "key" });

            var service = new GetAddressAddressSearchService(httpClient, options.Object);

            // Act
            var result = await service.SearchByPostcode("XX2 00X");

            // Assert
            Assert.Equal(3, result.Count);

            Assert.Equal($"XX2 00X::658 Mitcham Road", result.First().Id);
            Assert.Equal("658 Mitcham Road", result.First().StreetAddress);
            Assert.Equal("Croydon", result.First().Place);

            Assert.Equal($"XX2 00X::660 Mitcham Road", result.Skip(1).First().Id);
            Assert.Equal("660 Mitcham Road", result.Skip(1).First().StreetAddress);
            Assert.Equal("Croydon", result.Skip(1).First().Place);

            Assert.Equal($"XX2 00X::Lanfranc School House Mitcham Road", result.Skip(2).First().Id);
            Assert.Equal("Lanfranc School House Mitcham Road", result.Skip(2).First().StreetAddress);
            Assert.Equal("Croydon", result.Skip(2).First().Place);
        }

        [Fact]
        public async Task SearchByPostcode_WithNotFoundResponse_ReturnsEmptyResult()
        {
            // Arrange
            var httpRequestInterceptor = new HttpClientInterceptorOptions();

            new HttpRequestInterceptionBuilder()
                .Requests()
                .ForHttps()
                .ForHost("example.com")
                .ForPath("getaddress/XX2 00X")
                .ForQuery("api-key=key&expand=true&sort=true")
                .Responds()
                .WithStatus(HttpStatusCode.NotFound)
                .RegisterWith(httpRequestInterceptor);

            var httpClient = httpRequestInterceptor.CreateHttpClient();

            var options = new Mock<IOptions<GetAddressAddressSearchServiceOptions>>();
            options.Setup(s => s.Value).Returns(new GetAddressAddressSearchServiceOptions { ApiUrl = "https://example.com/getaddress/{0}", ApiKey = "key" });

            var service = new GetAddressAddressSearchService(httpClient, options.Object);

            // Act
            var result = await service.SearchByPostcode("XX2 00X");

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task SearchByPostcode_WithBadRequest_ReturnsEmptyResult()
        {
            // Arrange
            var httpRequestInterceptor = new HttpClientInterceptorOptions();

            new HttpRequestInterceptionBuilder()
                .Requests()
                .ForHttps()
                .ForHost("example.com")
                .ForPath("getaddress/XX2 00X")
                .ForQuery("api-key=key&expand=true&sort=true")
                .Responds()
                .WithStatus(HttpStatusCode.BadRequest)
                .RegisterWith(httpRequestInterceptor);

            var httpClient = httpRequestInterceptor.CreateHttpClient();

            var options = new Mock<IOptions<GetAddressAddressSearchServiceOptions>>();
            options.Setup(s => s.Value).Returns(new GetAddressAddressSearchServiceOptions { ApiUrl = "https://example.com/getaddress/{0}", ApiKey = "key" });

            var service = new GetAddressAddressSearchService(httpClient, options.Object);

            // Act
            var result = await service.SearchByPostcode("XX2 00X");

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task SearchByPostcode_WithInternalServerError_ThrowsException()
        {
            // Arrange
            var httpRequestInterceptor = new HttpClientInterceptorOptions();

            new HttpRequestInterceptionBuilder()
                .Requests()
                .ForHttps()
                .ForHost("example.com")
                .ForPath("getaddress/XX2 00X")
                .ForQuery("api-key=key&expand=true&sort=true")
                .Responds()
                .WithStatus(HttpStatusCode.InternalServerError)
                .RegisterWith(httpRequestInterceptor);

            var httpClient = httpRequestInterceptor.CreateHttpClient();

            var options = new Mock<IOptions<GetAddressAddressSearchServiceOptions>>();
            options.Setup(s => s.Value).Returns(new GetAddressAddressSearchServiceOptions { ApiUrl = "https://example.com/getaddress/{0}", ApiKey = "key" });

            var service = new GetAddressAddressSearchService(httpClient, options.Object);

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => service.SearchByPostcode("XX2 00X"));
        }

        [Fact]
        public async Task GetById_WithValidRequest_ReturnsParsedResult()
        {
            // Arrange
            var httpRequestInterceptor = new HttpClientInterceptorOptions();

            new HttpRequestInterceptionBuilder()
                .Requests()
                .ForHttps()
                .ForHost("example.com")
                .ForPath("getaddress/XX2 00X")
                .ForQuery("api-key=key&expand=true&sort=true")
                .Responds()
                .WithJsonContent(new
                {
                    postcode = "XX2 00X",
                    latitude = 51.39020538330078,
                    longitude = -0.1320359706878662,
                    addresses = new[]
                    {
                        new
                        {
                            formatted_address = new[] { "658 Mitcham Road", "", "", "Croydon", "Surrey" },
                            thoroughfare = "Mitcham Road",
                            building_name = "",
                            sub_building_name = "",
                            sub_building_number = "",
                            building_number = "658",
                            line_1 = "658 Mitcham Road",
                            line_2 = "",
                            line_3 = "",
                            line_4 = "",
                            locality = "",
                            town_or_city = "Croydon",
                            county = "Surrey",
                            district = "Croydon",
                            country = "England"
                        },
                        new
                        {
                            formatted_address = new[] { "660 Mitcham Road", "", "", "Croydon", "Surrey" },
                            thoroughfare = "Mitcham Road",
                            building_name = "",
                            sub_building_name = "",
                            sub_building_number = "",
                            building_number = "660",
                            line_1 = "660 Mitcham Road",
                            line_2 = "",
                            line_3 = "",
                            line_4 = "",
                            locality = "",
                            town_or_city = "Croydon",
                            county = "Surrey",
                            district = "Croydon",
                            country = "England"
                        },
                        new
                        {
                            formatted_address = new[] { "Lanfranc School House", "Mitcham Road", "", "Croydon", "Surrey" },
                            thoroughfare = "Mitcham Road",
                            building_name = "",
                            sub_building_name = "Lanfranc School House",
                            sub_building_number = "",
                            building_number = "",
                            line_1 = "Lanfranc School House",
                            line_2 = "Mitcham Road",
                            line_3 = "",
                            line_4 = "",
                            locality = "",
                            town_or_city = "Croydon",
                            county = "Surrey",
                            district = "Croydon",
                            country = "England"
                        }
                    }
                })
                .RegisterWith(httpRequestInterceptor);

            var httpClient = httpRequestInterceptor.CreateHttpClient();

            var options = new Mock<IOptions<GetAddressAddressSearchServiceOptions>>();
            options.Setup(s => s.Value).Returns(new GetAddressAddressSearchServiceOptions { ApiUrl = "https://example.com/getaddress/{0}", ApiKey = "key" });

            var service = new GetAddressAddressSearchService(httpClient, options.Object);

            // Act
            var result = await service.GetById($"XX2 00X::660 Mitcham Road");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("660 Mitcham Road", result.Line1);
            Assert.Equal("", result.Line2);
            Assert.Equal("", result.Line3);
            Assert.Equal("", result.Line4);
            Assert.Equal("Croydon", result.PostTown);
            Assert.Equal("Surrey", result.County);
            Assert.Equal("XX2 00X", result.Postcode);
            Assert.Equal("England", result.CountryName);
        }

        [Fact]
        public async Task GetById_ItemDoesNotExist_ReturnsNull()
        {
            // Arrange
            var httpRequestInterceptor = new HttpClientInterceptorOptions();

            new HttpRequestInterceptionBuilder()
                .Requests()
                .ForHttps()
                .ForHost("example.com")
                .ForPath("getaddress/XX2 00X")
                .ForQuery("api-key=key&expand=true&sort=true")
                .Responds()
                .WithStatus(HttpStatusCode.NotFound)
                .RegisterWith(httpRequestInterceptor);

            var httpClient = httpRequestInterceptor.CreateHttpClient();

            var options = new Mock<IOptions<GetAddressAddressSearchServiceOptions>>();
            options.Setup(s => s.Value).Returns(new GetAddressAddressSearchServiceOptions { ApiUrl = "https://example.com/getaddress/{0}", ApiKey = "key" });

            var service = new GetAddressAddressSearchService(httpClient, options.Object);

            // Act
            var result = await service.GetById("XX2 00X::660 Mitcham Road");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetById_ErrorResponse_ThrowsException()
        {
            // Arrange
            var httpRequestInterceptor = new HttpClientInterceptorOptions();

            new HttpRequestInterceptionBuilder()
                .Requests()
                .ForHttps()
                .ForHost("example.com")
                .ForPath("getaddress/XX2 00X")
                .ForQuery("api-key=key&expand=true&sort=true")
                .IgnoringQuery()
                .Responds()
                .WithStatus(HttpStatusCode.InternalServerError)
                .RegisterWith(httpRequestInterceptor);

            var httpClient = httpRequestInterceptor.CreateHttpClient();

            var options = new Mock<IOptions<GetAddressAddressSearchServiceOptions>>();
            options.Setup(s => s.Value).Returns(new GetAddressAddressSearchServiceOptions { ApiUrl = "https://example.com/getaddress/{0}", ApiKey = "key" });

            var service = new GetAddressAddressSearchService(httpClient, options.Object);

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => service.GetById("XX2 00X::660 Mitcham Road"));
        }
    }
}
