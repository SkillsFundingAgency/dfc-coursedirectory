using System;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Tests.Core;
using Dfc.CourseDirectory.WebV2.ViewComponents.GdsPagination;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.ViewComponentTests.GdsPaginationTests
{
    public class GdsPaginationTests : MvcTestBase
    {
        public GdsPaginationTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task SinglePage_DoesNotRenderPagination()
        {
            var response = await HttpClient.GetAsync(
                "gdspaginationtests?currentPage=1&totalPages=1&totalItems=5&pageSize=10");

            response.EnsureSuccessStatusCode();
            var doc = await response.GetDocument();
            doc.QuerySelector(".govuk-pagination").Should().BeNull();
        }

        [Fact]
        public async Task ZeroItems_DoesNotRenderPagination()
        {
            var response = await HttpClient.GetAsync(
                "gdspaginationtests?currentPage=1&totalPages=1&totalItems=0&pageSize=10");

            response.EnsureSuccessStatusCode();
            var doc = await response.GetDocument();
            doc.QuerySelector(".govuk-pagination").Should().BeNull();
        }

        [Fact]
        public async Task MultiplePages_RendersPaginationElement()
        {
            var response = await HttpClient.GetAsync(
                "gdspaginationtests?currentPage=1&totalPages=5&totalItems=50&pageSize=10");

            response.EnsureSuccessStatusCode();
            var doc = await response.GetDocument();
            doc.QuerySelector(".govuk-pagination").Should().NotBeNull();
        }

        [Fact]
        public async Task Page1_DoesNotRenderPreviousLink()
        {
            var response = await HttpClient.GetAsync(
                "gdspaginationtests?currentPage=1&totalPages=5&totalItems=50&pageSize=10");

            response.EnsureSuccessStatusCode();
            var doc = await response.GetDocument();
            doc.QuerySelector(".govuk-pagination__prev").Should().BeNull();
        }

        [Fact]
        public async Task PageAfterFirst_RendersPreviousLinkWithCorrectHref()
        {
            var response = await HttpClient.GetAsync(
                "gdspaginationtests?currentPage=3&totalPages=5&totalItems=50&pageSize=10");

            response.EnsureSuccessStatusCode();
            var doc = await response.GetDocument();
            var prevLink = doc.QuerySelector(".govuk-pagination__prev a");
            prevLink.Should().NotBeNull();
            prevLink.GetAttribute("href").Should().Be("?page=2");
        }

        [Fact]
        public async Task LastPage_DoesNotRenderNextLink()
        {
            var response = await HttpClient.GetAsync(
                "gdspaginationtests?currentPage=5&totalPages=5&totalItems=50&pageSize=10");

            response.EnsureSuccessStatusCode();
            var doc = await response.GetDocument();
            doc.QuerySelector(".govuk-pagination__next").Should().BeNull();
        }

        [Fact]
        public async Task PageBeforeLast_RendersNextLinkWithCorrectHref()
        {
            var response = await HttpClient.GetAsync(
                "gdspaginationtests?currentPage=3&totalPages=5&totalItems=50&pageSize=10");

            response.EnsureSuccessStatusCode();
            var doc = await response.GetDocument();
            var nextLink = doc.QuerySelector(".govuk-pagination__next a");
            nextLink.Should().NotBeNull();
            nextLink.GetAttribute("href").Should().Be("?page=4");
        }

        [Fact]
        public async Task CurrentPage_MarkedWithCurrentClass()
        {
            var response = await HttpClient.GetAsync(
                "gdspaginationtests?currentPage=3&totalPages=5&totalItems=50&pageSize=10");

            response.EnsureSuccessStatusCode();
            var doc = await response.GetDocument();
            var currentItems = doc.QuerySelectorAll(".govuk-pagination__item--current").ToList();
            currentItems.Should().HaveCount(1);
            currentItems[0].TextContent.Trim().Should().Be("3");
        }

        [Fact]
        public async Task SevenOrFewerPages_NoEllipsisRendered()
        {
            var response = await HttpClient.GetAsync(
                "gdspaginationtests?currentPage=4&totalPages=7&totalItems=70&pageSize=10");

            response.EnsureSuccessStatusCode();
            var doc = await response.GetDocument();
            doc.QuerySelector(".govuk-pagination__item--ellipsis").Should().BeNull();
        }

        [Fact]
        public async Task CurrentPageInMiddleOfMany_BothEllipsesRendered()
        {
            var response = await HttpClient.GetAsync(
                "gdspaginationtests?currentPage=5&totalPages=10&totalItems=100&pageSize=10");

            response.EnsureSuccessStatusCode();
            var doc = await response.GetDocument();
            var ellipses = doc.QuerySelectorAll(".govuk-pagination__item--ellipsis").ToList();
            ellipses.Should().HaveCount(2);
        }

        [Fact]
        public async Task CustomPageUrlFormat_UsedInLinks()
        {
            var format = Uri.EscapeDataString("/search?page={0}");
            var response = await HttpClient.GetAsync(
                $"gdspaginationtests?currentPage=2&totalPages=3&totalItems=3&pageSize=1&pageUrlFormat={format}");

            response.EnsureSuccessStatusCode();
            var doc = await response.GetDocument();
            doc.QuerySelector(".govuk-pagination__prev a")
                .GetAttribute("href").Should().Be("/search?page=1");
            doc.QuerySelector(".govuk-pagination__next a")
                .GetAttribute("href").Should().Be("/search?page=3");
        }
    }

    public class GdsPaginationTestsController : Controller
    {
        [HttpGet("gdspaginationtests")]
        public IActionResult Get(
            int currentPage,
            int totalPages,
            int totalItems,
            int pageSize,
            string pageUrlFormat = "?page={0}")
        {
            var model = new GdsPaginationModel
            {
                CurrentPage = currentPage,
                TotalPages = totalPages,
                TotalItems = totalItems,
                PageSize = pageSize
            };
            ViewBag.PageUrlFormat = pageUrlFormat;
            return View("~/ViewComponentTests/GdsPaginationTests/GdsPagination.cshtml", model);
        }
    }
}
