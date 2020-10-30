using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Dfc.CourseDirectory.FindACourseApi.Tests
{
    public class SwaggerDocTests : TestBase
    {
        public SwaggerDocTests(FindACourseApiApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task SwaggerDocIsServed()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "/swagger/v1/swagger.json");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType.MediaType.Should().Be("application/json");
        }
    }
}
