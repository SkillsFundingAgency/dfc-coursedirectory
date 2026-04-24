using System;
using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.WebV2.ViewComponents.GdsPagination;
using FluentAssertions;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.ViewComponentTests.GdsPaginationTests
{
    public class GdsPaginationModelTests
    {
        [Fact]
        public void Paginate_NullSource_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => GdsPaginationModel.Paginate<int>(null, 1, 10));
            ex.ParamName.Should().Be("source");
        }

        [Fact]
        public void Paginate_PageSizeZero_ThrowsArgumentOutOfRangeException()
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => GdsPaginationModel.Paginate(new List<int>(), 1, 0));
            ex.ParamName.Should().Be("pageSize");
        }

        [Fact]
        public void Paginate_PageSizeNegative_ThrowsArgumentOutOfRangeException()
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => GdsPaginationModel.Paginate(new List<int>(), 1, -1));
            ex.ParamName.Should().Be("pageSize");
        }

        [Fact]
        public void Paginate_EmptySource_ReturnsTotalPagesOneAndEmptyItems()
        {
            var (items, pagination) = GdsPaginationModel.Paginate(new List<int>(), 1, 10);
            items.Should().BeEmpty();
            pagination.TotalPages.Should().Be(1);
            pagination.TotalItems.Should().Be(0);
            pagination.CurrentPage.Should().Be(1);
        }

        [Fact]
        public void Paginate_ExactMultiple_CalculatesTotalPagesCorrectly()
        {
            var source = Enumerable.Range(1, 20).ToList();
            var (_, pagination) = GdsPaginationModel.Paginate(source, 1, 10);
            pagination.TotalPages.Should().Be(2);
        }

        [Fact]
        public void Paginate_NonExactMultiple_CeilsCorrectly()
        {
            var source = Enumerable.Range(1, 21).ToList();
            var (_, pagination) = GdsPaginationModel.Paginate(source, 1, 10);
            pagination.TotalPages.Should().Be(3);
        }

        [Fact]
        public void Paginate_SingleItem_ReturnsTotalPagesOne()
        {
            var (_, pagination) = GdsPaginationModel.Paginate(new List<int> { 1 }, 1, 10);
            pagination.TotalPages.Should().Be(1);
        }

        [Fact]
        public void Paginate_PageSizeEqualsCount_ReturnsSinglePage()
        {
            var source = Enumerable.Range(1, 10).ToList();
            var (_, pagination) = GdsPaginationModel.Paginate(source, 1, 10);
            pagination.TotalPages.Should().Be(1);
        }

        [Fact]
        public void Paginate_FirstPage_ReturnsFirstSlice()
        {
            var source = Enumerable.Range(1, 10).ToList();
            var (items, _) = GdsPaginationModel.Paginate(source, 1, 3);
            items.Should().Equal(1, 2, 3);
        }

        [Fact]
        public void Paginate_MiddlePage_ReturnsCorrectSlice()
        {
            var source = Enumerable.Range(1, 10).ToList();
            var (items, _) = GdsPaginationModel.Paginate(source, 2, 3);
            items.Should().Equal(4, 5, 6);
        }

        [Fact]
        public void Paginate_LastPage_ReturnsRemainingItems()
        {
            var source = Enumerable.Range(1, 10).ToList();
            var (items, _) = GdsPaginationModel.Paginate(source, 4, 3);
            items.Should().Equal(10);
        }

        [Fact]
        public void Paginate_PageZero_ClampedToPageOne()
        {
            var source = Enumerable.Range(1, 10).ToList();
            var (_, pagination) = GdsPaginationModel.Paginate(source, 0, 3);
            pagination.CurrentPage.Should().Be(1);
        }

        [Fact]
        public void Paginate_PageNegative_ClampedToPageOne()
        {
            var source = Enumerable.Range(1, 10).ToList();
            var (_, pagination) = GdsPaginationModel.Paginate(source, -99, 3);
            pagination.CurrentPage.Should().Be(1);
        }

        [Fact]
        public void Paginate_PageBeyondTotalPages_ClampedToLastPage()
        {
            var source = Enumerable.Range(1, 5).ToList();
            var (_, pagination) = GdsPaginationModel.Paginate(source, 99, 2);
            pagination.CurrentPage.Should().Be(3);
        }

        [Fact]
        public void Paginate_PageExactlyTotalPages_AcceptedAsIs()
        {
            var source = Enumerable.Range(1, 5).ToList();
            var (_, pagination) = GdsPaginationModel.Paginate(source, 3, 2);
            pagination.CurrentPage.Should().Be(3);
        }

        [Fact]
        public void FirstItemOnPage_EmptySource_ReturnsZero()
        {
            var model = new GdsPaginationModel { TotalItems = 0, CurrentPage = 1, PageSize = 10 };
            model.FirstItemOnPage.Should().Be(0);
        }

        [Fact]
        public void FirstItemOnPage_Page1_ReturnsOne()
        {
            var model = new GdsPaginationModel { TotalItems = 25, CurrentPage = 1, PageSize = 10 };
            model.FirstItemOnPage.Should().Be(1);
        }

        [Fact]
        public void FirstItemOnPage_Page2_ReturnsEleven()
        {
            var model = new GdsPaginationModel { TotalItems = 25, CurrentPage = 2, PageSize = 10 };
            model.FirstItemOnPage.Should().Be(11);
        }

        [Fact]
        public void LastItemOnPage_FullPage_ReturnsPageSizeMultiple()
        {
            var model = new GdsPaginationModel { TotalItems = 25, CurrentPage = 1, PageSize = 10 };
            model.LastItemOnPage.Should().Be(10);
        }

        [Fact]
        public void LastItemOnPage_PartialLastPage_ReturnsTotalItems()
        {
            var model = new GdsPaginationModel { TotalItems = 25, CurrentPage = 3, PageSize = 10 };
            model.LastItemOnPage.Should().Be(25);
        }

        [Fact]
        public void LastItemOnPage_EmptySource_ReturnsZero()
        {
            var model = new GdsPaginationModel { TotalItems = 0, CurrentPage = 1, PageSize = 10 };
            model.LastItemOnPage.Should().Be(0);
        }

        [Fact]
        public void HasPrevious_FirstPage_ReturnsFalse()
        {
            var model = new GdsPaginationModel { CurrentPage = 1, TotalPages = 5 };
            model.HasPrevious.Should().BeFalse();
        }

        [Fact]
        public void HasPrevious_SecondPage_ReturnsTrue()
        {
            var model = new GdsPaginationModel { CurrentPage = 2, TotalPages = 5 };
            model.HasPrevious.Should().BeTrue();
        }

        [Fact]
        public void HasNext_LastPage_ReturnsFalse()
        {
            var model = new GdsPaginationModel { CurrentPage = 5, TotalPages = 5 };
            model.HasNext.Should().BeFalse();
        }

        [Fact]
        public void HasNext_PageBeforeLast_ReturnsTrue()
        {
            var model = new GdsPaginationModel { CurrentPage = 4, TotalPages = 5 };
            model.HasNext.Should().BeTrue();
        }

        [Fact]
        public void HasNext_SinglePage_ReturnsFalse()
        {
            var model = new GdsPaginationModel { CurrentPage = 1, TotalPages = 1 };
            model.HasNext.Should().BeFalse();
        }

        [Fact]
        public void GetPageWindow_OnePageTotal_ReturnsSinglePage()
        {
            var model = new GdsPaginationModel { CurrentPage = 1, TotalPages = 1 };
            model.GetPageWindow().Should().Equal(new int?[] { 1 });
        }

        [Fact]
        public void GetPageWindow_SevenPageTotal_ReturnsAllPages()
        {
            var model = new GdsPaginationModel { CurrentPage = 4, TotalPages = 7 };
            var window = model.GetPageWindow().ToList();
            window.Should().Equal(new int?[] { 1, 2, 3, 4, 5, 6, 7 });
            window.Should().NotContain((int?)null);
        }

        [Fact]
        public void GetPageWindow_EightPageTotal_UsesEllipsisPath()
        {
            var model = new GdsPaginationModel { CurrentPage = 5, TotalPages = 8 };
            var window = model.GetPageWindow().ToList();
            window.Should().Contain((int?)null);
        }

        [Fact]
        public void GetPageWindow_AlwaysIncludesFirstAndLastPage()
        {
            var model = new GdsPaginationModel { CurrentPage = 5, TotalPages = 10 };
            var window = model.GetPageWindow().ToList();
            window.Should().Contain(1);
            window.Should().Contain(10);
        }

        [Fact]
        public void GetPageWindow_CurrentPage3_NoLeadingEllipsis()
        {
            var model = new GdsPaginationModel { CurrentPage = 3, TotalPages = 10 };
            var window = model.GetPageWindow().ToList();
            window[1].Should().NotBeNull("page 3 is not > 3, so no leading ellipsis");
        }

        [Fact]
        public void GetPageWindow_CurrentPage4_HasLeadingEllipsis()
        {
            var model = new GdsPaginationModel { CurrentPage = 4, TotalPages = 10 };
            var window = model.GetPageWindow().ToList();
            window[1].Should().BeNull("page 4 is > 3, so leading ellipsis expected");
        }

        [Fact]
        public void GetPageWindow_CurrentPage8_NoTrailingEllipsis()
        {
            var model = new GdsPaginationModel { CurrentPage = 8, TotalPages = 10 };
            var window = model.GetPageWindow().ToList();
            var lastTwo = window.TakeLast(2).ToList();
            lastTwo[0].Should().NotBeNull("8 < 10-2 is false, so no trailing ellipsis");
            lastTwo[1].Should().Be(10);
        }

        [Fact]
        public void GetPageWindow_CurrentPage7_HasTrailingEllipsis()
        {
            var model = new GdsPaginationModel { CurrentPage = 7, TotalPages = 10 };
            var window = model.GetPageWindow().ToList();
            var lastTwo = window.TakeLast(2).ToList();
            lastTwo[0].Should().BeNull("7 < 8, so trailing ellipsis expected");
            lastTwo[1].Should().Be(10);
        }

        [Fact]
        public void GetPageWindow_CurrentPageInMiddle_HasBothEllipses()
        {
            var model = new GdsPaginationModel { CurrentPage = 5, TotalPages = 10 };
            var window = model.GetPageWindow().ToList();
            window.Count(p => p == null).Should().Be(2);
        }

        [Fact]
        public void GetPageWindow_CurrentPage1_CorrectWindowPages()
        {
            var model = new GdsPaginationModel { CurrentPage = 1, TotalPages = 10 };
            var nonNull = model.GetPageWindow().Where(p => p != null).ToList();
            nonNull.Should().Equal(new int?[] { 1, 2, 10 });
        }

        [Fact]
        public void GetPageWindow_CurrentPage2_CorrectWindowPages()
        {
            var model = new GdsPaginationModel { CurrentPage = 2, TotalPages = 10 };
            var nonNull = model.GetPageWindow().Where(p => p != null).ToList();
            nonNull.Should().Equal(new int?[] { 1, 2, 3, 10 });
        }

        [Fact]
        public void GetPageWindow_CurrentPage5_CorrectWindowPages()
        {
            var model = new GdsPaginationModel { CurrentPage = 5, TotalPages = 10 };
            var nonNull = model.GetPageWindow().Where(p => p != null).ToList();
            nonNull.Should().Equal(new int?[] { 1, 4, 5, 6, 10 });
        }

        [Fact]
        public void GetPageWindow_CurrentPage9_CorrectWindowPages()
        {
            var model = new GdsPaginationModel { CurrentPage = 9, TotalPages = 10 };
            var nonNull = model.GetPageWindow().Where(p => p != null).ToList();
            nonNull.Should().Equal(new int?[] { 1, 8, 9, 10 });
        }
    }
}
