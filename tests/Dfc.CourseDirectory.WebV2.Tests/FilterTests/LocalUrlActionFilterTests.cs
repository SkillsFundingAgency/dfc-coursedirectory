using System.Net;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FilterTests
{
    public class LocalUrlActionFilterTests : TestBase
    {
        public LocalUrlActionFilterTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Theory]
        [InlineData(null)]
        [InlineData("foo")]
        [InlineData("http://google.com")]
        public async Task InvalidUrlParameterReturnsBadRequest(string param)
        {
            // Arrange

            // Act
            var response = await HttpClient.GetAsync(
                $"ValidateLocalUrlActionFilterTests?returnUrl={(param != null ? UrlEncoder.Default.Encode(param) : null)}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ValidUrlReturnsOk()
        {
            // Arrange

            // Act
            var response = await HttpClient.GetAsync("ValidateLocalUrlActionFilterTests?returnUrl=%2F");

            // Assert
            response.EnsureSuccessStatusCode();
        }
    }

    [Route("ValidateLocalUrlActionFilterTests")]
    public class ValidateLocalUrlActionFilterTestsController : Controller
    {
        [HttpGet]
        public IActionResult Get([LocalUrl] string returnUrl) => new EmptyResult();
    }
}
