using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Dom;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.ViewComponentTest.SharedViews
{
    public class PagerTests : MvcTestBase
    {
        public PagerTests(CourseDirectoryApplicationFactory factory)
          : base(factory)
        {
        }

        [Fact]
        public async Task RendersPage()
        {
            // Arrange

            // Act
            var response = await HttpClient.GetAsync("PagerTests?currentPage=6&totalPages=10&getPageUrl=1");

            // Assert
            var doc = await response.GetDocument();
            Assert.NotNull(doc.GetElementsByClassName("page-numbers-container pagination-container"));
        }

        [Fact]
        public async Task CurrentPageIsFirstPage_DoesNotRenderPreviousPageLink()
        {
            // Arrange

            // Act
            var response = await HttpClient.GetAsync("PagerTests?currentPage=1&totalPages=10&getPageUrl=1");

            // Assert
            var doc = await response.GetDocument();
            Assert.Empty(doc.GetElementsByClassName("pttcd-c-pager-previous"));
        }

        [Fact]
        public async Task CurrentPageIsNotFirst_RendersPreviousPageLink()
        {
            // Arrange

            // Act
            var response = await HttpClient.GetAsync("PagerTests?currentPage=5&totalPages=10&getPageUrl=1");

            // Assert
            var doc = await response.GetDocument();
            Assert.NotNull(doc.GetElementsByClassName("pttcd-pager__previouspage"));
        }
        [Fact]
        public async Task CurrentPageIsGreaterThan3_RendersEllipsis()
        {
            // Arrange

            // Act
            var response = await HttpClient.GetAsync("PagerTests?currentPage=4&totalPages=10&getPageUrl=1");

            // Assert
            var doc = await response.GetDocument();
            Assert.NotNull(doc.GetElementsByClassName("pttcd-c-Pager__ellipsis__first"));          
        }
        [Fact]
        public async Task CurrentPageIsFirstPage_DoesNotRenderEllipsis()
        {
            // Arrange

            // Act
            var response = await HttpClient.GetAsync("PagerTests?currentPage=1&totalPages=10&getPageUrl=1");

            // Assert
            var doc = await response.GetDocument();
            Assert.Empty(doc.GetElementsByClassName("pttcd-c-Pager__Ellipsis__First"));
        }

        [Fact]
        public async Task CurrentPageIsSix_RendersNextPageLink()
        {
            // Arrange

            // Act
            var response = await HttpClient.GetAsync("PagerTests?currentPage=6&totalPages=10&getPageUrl=1");

            // Assert
            var doc = await response.GetDocument();
            Assert.NotNull(doc.GetElementsByClassName("pttcd-pager__nextpage"));
        }

        [Fact]
        public async Task CurrentPageIsNotLast_RendersNextPageLink()
        {
            // Arrange

            // Act
            var response = await HttpClient.GetAsync("PagerTests?currentPage=10&totalPages=10&getPageUrl=1");

            // Assert
            var doc = await response.GetDocument();
            Assert.Null(doc.GetElementById("pttcd-pager__nextage"));
        }

        [Fact]
        public async Task RendersCurrentPage()
        {
            // Arrange

            // Act
            var response = await HttpClient.GetAsync("PagerTests?currentPage=4&totalPages=10&getPageUrl=1");

            // Assert
            var doc = await response.GetDocument();
            var currentPage = doc.GetElementsByClassName("pttcd-c-pager__currentpage pagination__item-current");
            Assert.Equal("4", currentPage.First().Text());
        }

    }

    public class PagerController : Controller
    {
        [HttpGet("PagerTests")]
        public IActionResult Get(int currentPage, int totalPages, int getPageUrl)
        {
            ViewBag.currentPage = currentPage;
            ViewBag.totalPages = totalPages;
            return View("~/ViewComponentTests/SharedViews/PagerTests.cshtml");
        }
    }
}

