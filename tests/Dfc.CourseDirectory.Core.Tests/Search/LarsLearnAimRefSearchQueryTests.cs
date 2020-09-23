using System;
using Azure.Search.Documents.Models;
using Dfc.CourseDirectory.Core.Search;
using Dfc.CourseDirectory.Core.Search.Models;
using FluentAssertions;
using Xunit;

namespace Dfc.CourseDirectory.Core.Tests.Search
{
    public class LarsLearnAimRefSearchQueryTests
    {
        [Fact]
        public void GenerateSearchQuery_GeneratesExpectedSearchQuery()
        {
            var query = BuildTestQuery();

            var result = query.GenerateSearchQuery();

            result.SearchText.Should().Be("TestLearnAimRef");
            result.Options.SearchMode.Should().Be(SearchMode.All);
            result.Options.SearchFields.Should().Equal(new[] { nameof(Lars.LearnAimRef) });
            result.Options.Filter.Should().Be($"({nameof(Lars.CertificationEndDate)} ge {query.CertificationEndDateFilter.Value:O} or {nameof(Lars.CertificationEndDate)} eq null)");
            result.Options.Size.Should().Be(1);
        }

        [Fact]
        public void GenerateSearchQuery_WithNullCertificationEndDateFilter_GeneratesSearchQueryWithEmptyFilter()
        {
            var query = BuildTestQuery();
            query.CertificationEndDateFilter = null;

            var result = query.GenerateSearchQuery();

            result.SearchText.Should().Be("TestLearnAimRef");
            result.Options.SearchMode.Should().Be(SearchMode.All);
            result.Options.SearchFields.Should().Equal(new[] { nameof(Lars.LearnAimRef) });
            result.Options.Filter.Should().BeEmpty();
            result.Options.Size.Should().Be(1);
        }

        private static LarsLearnAimRefSearchQuery BuildTestQuery()
        {
            return new LarsLearnAimRefSearchQuery
            {
                LearnAimRef = "TestLearnAimRef",
                CertificationEndDateFilter = DateTimeOffset.UtcNow
            };
        }
    }
}