using System;
using System.Net;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Api.Controllers;
using Dfc.CourseDirectory.FindACourseApi.Features.GetCourses;
using Dfc.CourseDirectory.FindACourseApi.Features.GetCourseUpdates;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Dfc.CourseDirectory.FindACourseApi.Tests.FeatureTests
{
    public class GetCourseUpdatesTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<GetCourseDataController>> _log;
        private readonly DateTime _cutOffDate = DateTime.UtcNow.AddDays(-14);
        private readonly GetCourseDataController _controller;
        public GetCourseUpdatesTests()
        {
            _mediator = new Mock<IMediator>();
            _log = new Mock<ILogger<GetCourseDataController>>();
            _controller = new GetCourseDataController(_mediator.Object, _log.Object);  
        }

        [Fact]
        public async Task InvalidPageSizePageNumber_ReturnsBadRequest()
        {
            // Arrange
            var pageSize = 0;
            var pageNumber = 0;
            
            // Act
            var response = await _controller.CourseUpdates(_cutOffDate, pageSize, pageNumber );
            var result = Assert.IsType<BadRequestObjectResult>(response);

            // Assert
            result.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            result.Value.Should().Be("PageSize and PageNumber must be greater than zero.");
        }
        [Fact]
        public async Task InvalidMinCutOffDate_ReturnsBadRequest()
        {
            // Arrange
            var pageSize = 0;
            var pageNumber = 0;

            // Act
            var response = await _controller.CourseUpdates(DateTime.MinValue, pageSize, pageNumber);
            var result = Assert.IsType<BadRequestObjectResult>(response);

            // Assert
            result.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            result.Value.Should().Be("PageSize and PageNumber must be greater than zero.");
        }
        [Fact]
        public async Task InvalidMaxCutOffDate_ReturnsBadRequest()
        {
            // Arrange
            var pageSize = 0;
            var pageNumber = 0;

            // Act
            var response = await _controller.CourseUpdates(DateTime.MaxValue, pageSize, pageNumber);
            var result = Assert.IsType<BadRequestObjectResult>(response);

            // Assert
            result.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            result.Value.Should().Be("PageSize and PageNumber must be greater than zero.");
        }
        [Fact]
        public async Task InvalidFutureCutOffDate_ReturnsBadRequest()
        {
            // Arrange
            var pageSize = 0;
            var pageNumber = 0;

            // Act
            var response = await _controller.CourseUpdates(DateTime.UtcNow.AddDays(1), pageSize, pageNumber);
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

            // Act
            var response = await _controller.CourseUpdates(_cutOffDate, pageSize, pageNumber); 

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

            // Act
            var response = await _controller.CourseUpdates(_cutOffDate, pageSize, pageNumber);

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
            _mediator.Setup(m => m.Send(It.IsAny<CourseUpdateRequest>(), default)).ReturnsAsync(new CourseUpdateResponse());
            // Act
            var response = await _controller.CourseUpdates(_cutOffDate, pageSize, pageNumber);

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

            // Act
            var response = await _controller.CourseUpdates(_cutOffDate, pageSize, pageNumber);

            var result = Assert.IsType<NotFoundResult>(response);

            // Assert
            result.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        }
    }
}
