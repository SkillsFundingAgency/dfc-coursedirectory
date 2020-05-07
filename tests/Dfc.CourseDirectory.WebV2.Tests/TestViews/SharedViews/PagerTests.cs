using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Dom;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.TestViews.SharedViews
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
            var response = await HttpClient.GetAsync("PagerContext?currentPage=6&totalPages=10&getPageUrl=1");

            // Assert
            var doc = await response.GetDocument();
            Assert.NotNull(doc.GetElementsByClassName("page-numbers-container pagination-container"));
        }

        [Fact]
        public async Task Pager_DoNotShowPreviousPageLinkIfCurrentPageisFirstPage()
        {
            // Arrange

            // Act
            var response = await HttpClient.GetAsync("PagerContext?currentPage=1&totalPages=10&getPageUrl=1");

            // Assert
            var doc = await response.GetDocument();
            Assert.Null(doc.GetElementById("pttcd-pager__PreviousPage"));
        }

        [Fact]
        public async Task Pager_ShowPreviousPageLinkIfCurrentPageisNumberFive()
        {
            // Arrange

            // Act
            var response = await HttpClient.GetAsync("PagerContext?currentPage=5&totalPages=10&getPageUrl=1");

            // Assert
            var doc = await response.GetDocument();
            Assert.NotNull(doc.GetElementById("pttcd-pager__PreviousPage"));
        }
        [Fact]
        public async Task Pager_VerifyEllipsisAreAddedWhenCurrentPageNumberisFour()
        {
            // Arrange

            // Act
            var response = await HttpClient.GetAsync("PagerContext?currentPage=4&totalPages=10&getPageUrl=1");

            // Assert
            var doc = await response.GetDocument();
            Assert.NotNull(doc.GetElementsByClassName("pttcd-c-Pager__Ellipsis__First"));
            Assert.NotNull(doc.GetElementsByClassName("pttcd-c-Pager__Ellipsis__Second"));
        }
        [Fact]
        public async Task Pager_VerifyEllipsisAreNotAddedWhenCurrentPageNumberisFirstPage()
        {
            // Arrange

            // Act
            var response = await HttpClient.GetAsync("PagerContext?currentPage=1&totalPages=10&getPageUrl=1");

            // Assert
            var doc = await response.GetDocument();
            Assert.Empty(doc.GetElementsByClassName("pttcd-c-Pager__Ellipsis__First"));
        }

        [Fact]
        public async Task Pager_ShowNextPageLinkIfCurrentPageNumberIsSix()
        {
            // Arrange

            // Act
            var response = await HttpClient.GetAsync("PagerContext?currentPage=6&totalPages=10&getPageUrl=1");

            // Assert
            var doc = await response.GetDocument();
            Assert.NotNull(doc.GetElementById("pttcd-pager__NextPage"));
        }

        [Fact]
        public async Task Pager_DoNotShowNextPageLinkIfCurrentPageIsTheLastPage()
        {
            // Arrange

            // Act
            var response = await HttpClient.GetAsync("PagerContext?currentPage=10&totalPages=10&getPageUrl=1");

            // Assert
            var doc = await response.GetDocument();
            Assert.Null(doc.GetElementById("pttcd-pager__NextPage"));
        }

        [Fact]
        public async Task Pager_RenderCurrentPage()
        {
            // Arrange

            // Act
            var response = await HttpClient.GetAsync("PagerContext?currentPage=4&totalPages=10&getPageUrl=1");

            // Assert
            var doc = await response.GetDocument();
            var currentPage = doc.GetElementById("pttcd-pager__CurrentPage").Text();
            Assert.Equal("4", currentPage);
        }

    }

    public class PagerController : Controller
    {
        [HttpGet("PagerContext")]
        public IActionResult Get(int currentPage, int totalPages, int getPageUrl)
        {
            ViewBag.currentPage = currentPage;
            ViewBag.totalPages = totalPages;
            return View("~/TestViews/SharedViews/PagerContext.cshtml");
        }
    }
}

