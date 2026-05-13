using System.Net;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Api.Controllers;
using Dfc.CourseDirectory.FindACourseApi.Features.GetCourses;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Dfc.CourseDirectory.FindACourseApi.Tests.FeatureTests
{
    public class GetCourseListTests 
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<GetCourseDataController>> _log;
        public GetCourseListTests()
        {
            _mediator = new Mock<IMediator>();
            _log = new Mock<ILogger<GetCourseDataController>>();
        }

        [Fact]
        public async Task InvalidPageSizePageNumber_ReturnsBadRequest()
        {
            // Arrange
            var pageSize = 0;
            var pageNumber = 0;
            var controller = new GetCourseDataController(_mediator.Object, _log.Object);
            
            // Act
            var response = await controller.CoursesList(pageSize, pageNumber);

            var result = Assert.IsType<BadRequestObjectResult>(response);

            // Assert
            result.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            result.Value.Should().Be("PageSize and PageNumber must be greater than zero.");
        }

        [Fact]
        public async Task InvalidPageSize_ReturnsBadRequest()
        {
            // Arrange
            var pageSize = 101;
            var pageNumber = 1;
            var controller = new GetCourseDataController(_mediator.Object, _log.Object);

            // Act
            var response = await controller.CoursesList(pageSize, pageNumber);

            var result = Assert.IsType<BadRequestObjectResult>(response);

            // Assert
            result.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            result.Value.Should().Be("PageSize must not exceed 100.");
        }

        [Fact]
        public async Task InvalidPageNumber_ReturnsBadRequest()
        {
            // Arrange
            var pageSize = 10;
            var pageNumber = -1;
            var controller = new GetCourseDataController(_mediator.Object, _log.Object);

            // Act
            var response = await controller.CoursesList(pageSize, pageNumber);

            var result = Assert.IsType<BadRequestObjectResult>(response);

            // Assert
            result.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            result.Value.Should().Be("PageSize and PageNumber must be greater than zero.");
        }

        [Fact]
        public async Task ValidPageNumberPageSize_ReturnsOk()
        {
            // Arrange
            var pageSize = 10;
            var pageNumber = 1;
            var controller = new GetCourseDataController(_mediator.Object, _log.Object);
            _mediator.Setup(m => m.Send(It.IsAny<CourseRequest>(), default)).ReturnsAsync(new CourseResponse());
            // Act
            var response = await controller.CoursesList(pageSize, pageNumber);

            var result = Assert.IsType<OkObjectResult>(response);

            // Assert
            result.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }

        [Fact]
        public async Task ValidPageNumberPageSize_ReturnsNotFound()
        {
            // Arrange
            var pageSize = 10;
            var pageNumber = 1;
            var controller = new GetCourseDataController(_mediator.Object, _log.Object);

            // Act
            var response = await controller.CoursesList(pageSize, pageNumber);

            var result = Assert.IsType<NotFoundResult>(response);

            // Assert
            result.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        }
    }
}
