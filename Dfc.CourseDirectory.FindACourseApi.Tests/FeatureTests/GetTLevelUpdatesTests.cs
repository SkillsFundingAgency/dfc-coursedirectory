using System;
using System.Net;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Api.Controllers;
using Dfc.CourseDirectory.FindACourseApi.Features.GetTLevelUpdates;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Dfc.CourseDirectory.FindACourseApi.Tests.FeatureTests
{
    public class GetTLevelUpdatesTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<GetTLevelsDataController>> _log;
        private readonly string _cutOffdateString;
        private readonly string _futureCutOffDateString;
        private readonly GetTLevelsDataController _controller;
        public GetTLevelUpdatesTests()
        {
            _mediator = new Mock<IMediator>();
            _log = new Mock<ILogger<GetTLevelsDataController>>();
            _controller = new GetTLevelsDataController(_mediator.Object, _log.Object);
            _cutOffdateString = DateTime.Now.AddDays(-14).ToString("yyyy-MM-ddTHH:mm:ss");
            _futureCutOffDateString = DateTime.Now.AddDays(1).ToString("yyyy-MM-ddTHH:mm:ss");
        }

        [Fact]
        public async Task InvalidPageSizePageNumber_ReturnsBadRequest()
        {
            // Arrange
            var pageSize = 0;
            var pageNumber = 0;
            
            // Act
            var response = await _controller.TLevelUpdates(_cutOffdateString, pageSize, pageNumber );
            var result = Assert.IsType<BadRequestObjectResult>(response);

            // Assert
            result.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            result.Value.Should().Be("PageSize and PageNumber must be greater than zero.");
        }
        
        [Theory]        
        [InlineData("2025-12-01")]
        [InlineData("2025-12-01T00:00:00")]
        [InlineData("2025-12-01T13:00:00")]
        public async Task ValidCutOffDateFormat_ReturnsOK(string validDate)
        {
            // Arrange
            var pageSize = 1;
            var pageNumber = 1;
            _mediator.Setup(m => m.Send(It.IsAny<TLevelUpdateRequest>(), default)).ReturnsAsync(new TLevelUpdateResponse());

            // Act
            var response = await _controller.TLevelUpdates(validDate, pageSize, pageNumber);
            var result = Assert.IsType<OkObjectResult>(response);

            // Assert
            result.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }

        [Theory]
        [InlineData("this is invalid date string")]
        [InlineData("24/11/2025")]
        [InlineData("24/11/2025 00:00:00")]
        [InlineData("24/11/2025 13:00:00")]
        public async Task InValidCutOffDateFormat_ReturnsBadRequest(string invalidDate)
        {
            // Arrange
            var pageSize = 1;
            var pageNumber = 1;
            // Act
            var response = await _controller.TLevelUpdates(invalidDate, pageSize, pageNumber);
            var result = Assert.IsType<BadRequestObjectResult>(response);

            // Assert
            result.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            result.Value.Should().Be("CutOffDate must be a valid date.");
        }
        [Fact]
        public async Task InvalidFutureCutOffDate_ReturnsBadRequest()
        {
            // Arrange
            var pageSize = 1;
            var pageNumber = 1;

            // Act
            var response = await _controller.TLevelUpdates(_futureCutOffDateString, pageSize, pageNumber);
            var result = Assert.IsType<BadRequestObjectResult>(response);

            // Assert
            result.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            result.Value.Should().Be("CutOffDate cannot be a future date.");
        }

        [Fact]
        public async Task InvalidPageSize_ReturnsBadRequest()
        {
            // Arrange
            var pageSize = 101;
            var pageNumber = 1;

            // Act
            var response = await _controller.TLevelUpdates(_cutOffdateString, pageSize, pageNumber); 

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
            var response = await _controller.TLevelUpdates(_cutOffdateString, pageSize, pageNumber);

            var result = Assert.IsType<BadRequestObjectResult>(response);

            // Assert
            result.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            result.Value.Should().Be("PageSize and PageNumber must be greater than zero.");
        }

        [Fact]
        public async Task ValidCutOffDatePageNumberPageSize_ReturnsOk()
        {
            // Arrange
            var pageSize = 10;
            var pageNumber = 1;
            _mediator.Setup(m => m.Send(It.IsAny<TLevelUpdateRequest>(), default)).ReturnsAsync(new TLevelUpdateResponse());
            // Act
            var response = await _controller.TLevelUpdates(_cutOffdateString, pageSize, pageNumber);

            var result = Assert.IsType<OkObjectResult>(response);

            // Assert
            result.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }

        [Fact]
        public async Task ValidCutOffDatePageNumberPageSize_ReturnsNotFound()
        {
            // Arrange
            var pageSize = 10;
            var pageNumber = 1;

            // Act
            var response = await _controller.TLevelUpdates(_cutOffdateString, pageSize, pageNumber);

            var result = Assert.IsType<NotFoundResult>(response);

            // Assert
            result.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        }
    }
}
