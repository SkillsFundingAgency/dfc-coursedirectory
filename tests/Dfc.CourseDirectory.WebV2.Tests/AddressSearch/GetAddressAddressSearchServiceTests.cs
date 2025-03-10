using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.AddressSearch;
using JustEat.HttpClientInterception;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using static Dfc.CourseDirectory.WebV2.AddressSearch.GetAddressAddressSearchService;

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
                .ForQuery("key=key")
                .Responds()
                .WithJsonContent(new
                {
                    header = new
                    {
                        uri = "https://example.com/getaddress/{0}",
                        query = "",
                        offset = 1,
                        totalresults = 1,
                        format = "",
                        dataset = "",
                        lr = "",
                        maxresults = 1,
                        lastupdate = "2024-04-19",
                        output_srs = "",
                    },
                    results = new[]
                    {
                        new
                        {
                            DPA = new
                            {
                                UPRN = 1,
                                USRN = 1,
                                LPI_KEY = "",
                                //PAO_START_NUMBER = 1,
                                Address = "658 Mitcham Road",
                                STREET_DESCRIPTION = "",
                                POST_TOWN= "Croydon",
                                TOWN_NAME = "Croydon"
                            }
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
            Assert.Single(result);

            Assert.Equal($"XX2 00X::1", result.First().Id);
            Assert.Equal("658 Mitcham Road", result.First().StreetAddress);
            Assert.Equal("Croydon", result.First().Place);

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
                .ForQuery("key=key")
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
                .ForQuery("key=key")
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
                .ForQuery("key=key")
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
               .ForQuery("key=key")
               .Responds()
               .WithJsonContent(new
               {
                   header = new
                   {
                       uri = "https://example.com/getaddress/{0}",
                       query = "",
                       offset = 1,
                       totalresults = 1,
                       format = "",
                       dataset = "",
                       lr = "",
                       maxresults = 1,
                       lastupdate = "2024-04-19",
                       output_srs = "",
                   },
                   results = new[]
                   {
                        new
                        {
                            DPA = new
                            {
                                UPRN = 1,
                                USRN = 1,
                                LPI_KEY = "",
                                //PAO_START_NUMBER = 1,
                                ADDRESS="660 Mitcham Road",
                                THOROUGHFARE_NAME="660 Mitcham Road",
                                 BUILDING_NUMBER="660",
                                 STREET_DESCRIPTION = "",
                                 POST_TOWN = "Croydon",

                                TOWN_NAME = "Croydon"

                            }
                        }
                   }
               })
               .RegisterWith(httpRequestInterceptor);
            var httpClient = httpRequestInterceptor.CreateHttpClient();

            var options = new Mock<IOptions<GetAddressAddressSearchServiceOptions>>();
            options.Setup(s => s.Value).Returns(new GetAddressAddressSearchServiceOptions { ApiUrl = "https://example.com/getaddress/{0}", ApiKey = "key" });

            var service = new GetAddressAddressSearchService(httpClient, options.Object);

            // Act
            var result = await service.GetById($"XX2 00X::1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("660 Mitcham Road", result.Line2);
            Assert.Equal("Croydon", result.PostTown);
            Assert.Equal("XX2 00X", result.Postcode);
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
                .ForQuery("key=key")
                .Responds()
                .WithStatus(HttpStatusCode.NotFound)
                .RegisterWith(httpRequestInterceptor);

            var httpClient = httpRequestInterceptor.CreateHttpClient();

            var options = new Mock<IOptions<GetAddressAddressSearchServiceOptions>>();
            options.Setup(s => s.Value).Returns(new GetAddressAddressSearchServiceOptions { ApiUrl = "https://example.com/getaddress/{0}", ApiKey = "key" });

            var service = new GetAddressAddressSearchService(httpClient, options.Object);

            // Act
            var result = await service.GetById("XX2 00X::1");

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
                .ForQuery("key=key")
                .IgnoringQuery()
                .Responds()
                .WithStatus(HttpStatusCode.InternalServerError)
                .RegisterWith(httpRequestInterceptor);

            var httpClient = httpRequestInterceptor.CreateHttpClient();

            var options = new Mock<IOptions<GetAddressAddressSearchServiceOptions>>();
            options.Setup(s => s.Value).Returns(new GetAddressAddressSearchServiceOptions { ApiUrl = "https://example.com/getaddress/{0}", ApiKey = "key" });

            var service = new GetAddressAddressSearchService(httpClient, options.Object);

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => service.GetById("XX2 00X::1"));
        }
    }
}

