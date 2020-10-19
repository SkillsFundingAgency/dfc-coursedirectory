using System;
using Azure.Search.Documents.Models;
using Dfc.CourseDirectory.Core.Search;
using Dfc.CourseDirectory.Core.Search.Models;
using FluentAssertions;
using Xunit;

namespace Dfc.CourseDirectory.Core.Tests.Search
{
    public class LarsSearchQueryTests
    {
        [Fact]
        public void GenerateSearchQuery_GeneratesExpectedSearchQuery()
        {
            var query = BuildTestQuery();

            var result = query.GenerateSearchQuery();

            result.SearchText.Should().Be("\"TestSearchTerm\" | (TestSearchTerm*)");
            result.Options.SearchMode.Should().Be(SearchMode.All);
            result.Options.SearchFields.Should().Equal(new[] { "TestSearchField1", "TestSearchFeild2", "TestSearchFeild3" });
            result.Options.Filter.Should().Be($"search.in(NotionalNVQLevelv2, 'NotionalNVQLevelv2Filter1|NotionalNVQLevelv2Filter2|NotionalNVQLevelv2Filter3', '|') and search.in(AwardOrgCode, 'TestAwardOrgCodeFilter1|TestAwardOrgCodeFilter2|TestAwardOrgCodeFilter3', '|') and search.in(AwardOrgAimRef, 'TestAwardOrgAimRefFilter1|TestAwardOrgAimRefFilter2|TestAwardOrgAimRefFilter3', '|') and search.in(SectorSubjectAreaTier1, 'TestSectorSubjectAreaTier1Filter1|TestSectorSubjectAreaTier1Filter2|TestSectorSubjectAreaTier1Filter3', '|') and search.in(SectorSubjectAreaTier2, 'TestSectorSubjectAreaTier2Filter1|SectorSubjectAreaTier2Filter2|SectorSubjectAreaTier2Filter3', '|') and ({nameof(Lars.CertificationEndDate)} ge {query.CertificationEndDateFilter.Value:O} or {nameof(Lars.CertificationEndDate)} eq null)");
            result.Options.Facets.Should().Equal(new[] { "TestFacet1", "TestFacet2", "TestFacet3" });
            result.Options.Size.Should().Be(12);
            result.Options.Skip.Should().Be(24);
            result.Options.IncludeTotalCount.Should().BeTrue();
        }

        [Theory]
        [InlineData(null, "*")]
        [InlineData("", "*")]
        [InlineData(" ", "*")]
        [InlineData("+++", "\\+\\+\\+*")]
        [InlineData("TestSearchText", "\"TestSearchText\" | (TestSearchText*)")]
        [InlineData(" TestSearchText", "\"TestSearchText\" | (TestSearchText*)")]
        [InlineData("TestSearchText ", "\"TestSearchText\" | (TestSearchText*)")]
        [InlineData("TestSearchTexts", "\"TestSearchTexts\" | (TestSearchText*)")]
        [InlineData("TestSearchText's", "\"TestSearchText\\'s\" | (TestSearchText*)")]
        [InlineData("TestSearchTexts'", "\"TestSearchTexts\\'\" | (TestSearchText*)")]
        [InlineData("Test Search Text", "\"Test Search Text\" | (Test* Search* Text*)")]
        [InlineData("Test Search Text ", "\"Test Search Text\" | (Test* Search* Text*)")]
        [InlineData("Test  Search Text", "\"Test  Search Text\" | (Test* Search* Text*)")]
        [InlineData("T!e-s@t (Search) T?e%xt", "\"T!e\\-s@t \\(Search\\) T\\?e%xt\" | (Test* Search* Text*)")]
        public void GenerateSearchQuery_WithSearchText_ReturnsQueryWithExpectedSearchText(string searchText, string expectedResult)
        {
            var query = BuildTestQuery();
            query.SearchText = searchText;

            var result = query.GenerateSearchQuery();

            result.SearchText.Should().Be(expectedResult);
            result.Options.SearchMode.Should().Be(SearchMode.All);
            result.Options.SearchFields.Should().Equal(new[] { "TestSearchField1", "TestSearchFeild2", "TestSearchFeild3" });
            result.Options.Filter.Should().Be($"search.in(NotionalNVQLevelv2, 'NotionalNVQLevelv2Filter1|NotionalNVQLevelv2Filter2|NotionalNVQLevelv2Filter3', '|') and search.in(AwardOrgCode, 'TestAwardOrgCodeFilter1|TestAwardOrgCodeFilter2|TestAwardOrgCodeFilter3', '|') and search.in(AwardOrgAimRef, 'TestAwardOrgAimRefFilter1|TestAwardOrgAimRefFilter2|TestAwardOrgAimRefFilter3', '|') and search.in(SectorSubjectAreaTier1, 'TestSectorSubjectAreaTier1Filter1|TestSectorSubjectAreaTier1Filter2|TestSectorSubjectAreaTier1Filter3', '|') and search.in(SectorSubjectAreaTier2, 'TestSectorSubjectAreaTier2Filter1|SectorSubjectAreaTier2Filter2|SectorSubjectAreaTier2Filter3', '|') and ({nameof(Lars.CertificationEndDate)} ge {query.CertificationEndDateFilter.Value:O} or {nameof(Lars.CertificationEndDate)} eq null)");
            result.Options.Facets.Should().Equal(new[] { "TestFacet1", "TestFacet2", "TestFacet3" });
            result.Options.Size.Should().Be(12);
            result.Options.Skip.Should().Be(24);
            result.Options.IncludeTotalCount.Should().BeTrue();
        }

        [Fact]
        public void GenerateSearchQuery_WithNullCertificationEndDateFilter_GeneratesSearchQueryWithEmptyFilter()
        {
            var query = BuildTestQuery();
            query.CertificationEndDateFilter = null;

            var result = query.GenerateSearchQuery();

            result.SearchText.Should().Be("\"TestSearchTerm\" | (TestSearchTerm*)");
            result.Options.SearchMode.Should().Be(SearchMode.All);
            result.Options.SearchFields.Should().Equal(new[] { "TestSearchField1", "TestSearchFeild2", "TestSearchFeild3" });
            result.Options.Filter.Should().Be($"search.in(NotionalNVQLevelv2, 'NotionalNVQLevelv2Filter1|NotionalNVQLevelv2Filter2|NotionalNVQLevelv2Filter3', '|') and search.in(AwardOrgCode, 'TestAwardOrgCodeFilter1|TestAwardOrgCodeFilter2|TestAwardOrgCodeFilter3', '|') and search.in(AwardOrgAimRef, 'TestAwardOrgAimRefFilter1|TestAwardOrgAimRefFilter2|TestAwardOrgAimRefFilter3', '|') and search.in(SectorSubjectAreaTier1, 'TestSectorSubjectAreaTier1Filter1|TestSectorSubjectAreaTier1Filter2|TestSectorSubjectAreaTier1Filter3', '|') and search.in(SectorSubjectAreaTier2, 'TestSectorSubjectAreaTier2Filter1|SectorSubjectAreaTier2Filter2|SectorSubjectAreaTier2Filter3', '|')");
            result.Options.Facets.Should().Equal(new[] { "TestFacet1", "TestFacet2", "TestFacet3" });
            result.Options.Size.Should().Be(12);
            result.Options.Skip.Should().Be(24);
            result.Options.IncludeTotalCount.Should().BeTrue();
        }

        [Fact]
        public void GenerateSearchQuery_WithNullPageNumber_GeneratesSearchQueryWithNullSkip()
        {
            var query = BuildTestQuery();
            query.PageNumber = null;

            var result = query.GenerateSearchQuery();

            result.SearchText.Should().Be("\"TestSearchTerm\" | (TestSearchTerm*)");
            result.Options.SearchMode.Should().Be(SearchMode.All);
            result.Options.SearchFields.Should().Equal(new[] { "TestSearchField1", "TestSearchFeild2", "TestSearchFeild3" });
            result.Options.Filter.Should().Be($"search.in(NotionalNVQLevelv2, 'NotionalNVQLevelv2Filter1|NotionalNVQLevelv2Filter2|NotionalNVQLevelv2Filter3', '|') and search.in(AwardOrgCode, 'TestAwardOrgCodeFilter1|TestAwardOrgCodeFilter2|TestAwardOrgCodeFilter3', '|') and search.in(AwardOrgAimRef, 'TestAwardOrgAimRefFilter1|TestAwardOrgAimRefFilter2|TestAwardOrgAimRefFilter3', '|') and search.in(SectorSubjectAreaTier1, 'TestSectorSubjectAreaTier1Filter1|TestSectorSubjectAreaTier1Filter2|TestSectorSubjectAreaTier1Filter3', '|') and search.in(SectorSubjectAreaTier2, 'TestSectorSubjectAreaTier2Filter1|SectorSubjectAreaTier2Filter2|SectorSubjectAreaTier2Filter3', '|') and ({nameof(Lars.CertificationEndDate)} ge {query.CertificationEndDateFilter.Value:O} or {nameof(Lars.CertificationEndDate)} eq null)");
            result.Options.Facets.Should().Equal(new[] { "TestFacet1", "TestFacet2", "TestFacet3" });
            result.Options.Size.Should().Be(12);
            result.Options.Skip.Should().Be(null);
            result.Options.IncludeTotalCount.Should().BeTrue();
        }

        private static LarsSearchQuery BuildTestQuery()
        {
            return new LarsSearchQuery
            {
                SearchText = "TestSearchTerm",
                SearchFields = new[] { "TestSearchField1", "TestSearchFeild2", "TestSearchFeild3" },
                NotionalNVQLevelv2Filters = new[] { "NotionalNVQLevelv2Filter1", "NotionalNVQLevelv2Filter2", "NotionalNVQLevelv2Filter3" },
                AwardOrgCodeFilters = new[] { "TestAwardOrgCodeFilter1", "TestAwardOrgCodeFilter2", "TestAwardOrgCodeFilter3" },
                AwardOrgAimRefFilters = new[] { "TestAwardOrgAimRefFilter1", "TestAwardOrgAimRefFilter2", "TestAwardOrgAimRefFilter3" },
                SectorSubjectAreaTier1Filters = new[] { "TestSectorSubjectAreaTier1Filter1", "TestSectorSubjectAreaTier1Filter2", "TestSectorSubjectAreaTier1Filter3" },
                SectorSubjectAreaTier2Filters = new[] { "TestSectorSubjectAreaTier2Filter1", "SectorSubjectAreaTier2Filter2", "SectorSubjectAreaTier2Filter3" },
                CertificationEndDateFilter = DateTimeOffset.UtcNow,
                Facets = new[] { "TestFacet1", "TestFacet2", "TestFacet3" },
                PageSize = 12,
                PageNumber = 3
            };
        }
    }
}