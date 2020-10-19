using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Core.Search.AzureSearch;
using FluentAssertions;
using Xunit;

namespace Dfc.CourseDirectory.Core.Tests.Search.AzureSearch
{
    public class SearchTextExtensionsTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void EscapeSimpleQuerySearchOperators_WithNullOrEmptySearchText_ReturnsEmptyString(string searchText)
        {
            var result = searchText.EscapeSimpleQuerySearchOperators();

            result.Should().Be(string.Empty);
        }

        [Theory]
        [InlineData(@"\", @"\\")]
        [InlineData(@"+", @"\+")]
        [InlineData(@"|", @"\|")]
        [InlineData(@"""", @"\""")]
        [InlineData(@"(", @"\(")]
        [InlineData(@")", @"\)")]
        [InlineData(@"'", @"\'")]
        [InlineData(@"*", @"\*")]
        [InlineData(@"?", @"\?")]
        [InlineData(@"#", "")]
        [InlineData(@"-", @"\-")]
        [InlineData(@"\+|()'-*?#", @"\\\+\|\(\)\'\-\*\?")]
        public void EscapeSimpleQuerySearchOperators_WithSearchTextContainingSimpleSearchQueryOperator_ReturnsEscapedSearchText(string searchText, string expectedResult)
        {
            var result = searchText.EscapeSimpleQuerySearchOperators();

            result.Should().Be(expectedResult);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void AppendWildcardWhenLastCharIsAlphanumeric_WithNullOrEmptySearchText_ReturnsEmptyString(string searchText)
        {
            var result = searchText.AppendWildcardWhenLastCharIsAlphanumeric();

            result.Should().Be(string.Empty);
        }

        [Theory]
        [InlineData("a")]
        [InlineData("B")]
        [InlineData("Test a")]
        [InlineData("Test B")]
        [InlineData("1")]
        [InlineData("Test 1")]
        public void AppendWildcardWhenLastCharIsAlphanumeric_WithLastCharAlphanumeric_ReturnsSearchTextWithWildcard(string searchText)
        {
            var result = searchText.AppendWildcardWhenLastCharIsAlphanumeric();

            result.Should().Be($"{searchText}*");
        }

        [Theory]
        [InlineData(" ")]
        [InlineData("Test ")]
        [InlineData("-")]
        [InlineData("Test-")]
        [InlineData("*")]
        [InlineData("Test*")]
        public void AppendWildcardWhenLastCharIsAlphanumeric_WithLastCharNotAlphanumeric_ReturnsSearchTextWithNoWildcard(string searchText)
        {
            var result = searchText.AppendWildcardWhenLastCharIsAlphanumeric();

            result.Should().Be(searchText);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void RemoveNonAlphanumericChars_WithNullOrEmptySearchText_ReturnsEmptyString(string searchText)
        {
            var result = searchText.RemoveNonAlphanumericChars();

            result.Should().Be(string.Empty);
        }

        [Fact]
        public void RemoveNonAlphanumericChars_RemovesNonAlphanumericOrWhiteSpaceChars()
        {
            var searchText = @"TestSearch!""£$%^&*()_+-=`{}[]:@~;'#<>?,./|\' Text";

            var result = searchText.RemoveNonAlphanumericChars();

            result.Should().Be("TestSearch Text");
        }

        [Fact]
        public void TransformWhen_WithNullPredicate_ThrowsException()
        {
            Func<string> action = () => "TestValue".TransformWhen(null, s => s);
            action.Should().ThrowExactly<ArgumentNullException>();
        }

        [Fact]
        public void TransformWhen_WithNullTransformation_ThrowsException()
        {
            Func<string> action = () => "TestValue".TransformWhen(_ => true, null);
            action.Should().ThrowExactly<ArgumentNullException>();
        }

        [Fact]
        public void TransformWhen_WithPredicateTrue_ReturnsTransformedValue()
        {
            var result = "TestValue".TransformWhen(v => v == "TestValue", v => $"{v}+Transform");
            result.Should().Be("TestValue+Transform");
        }

        [Fact]
        public void TransformWhen_WithPredicateFalse_ReturnsValue()
        {
            var result = "TestValue".TransformWhen(v => v != "TestValue", v => $"{v}+Transform");
            result.Should().Be("TestValue");
        }

        [Fact]
        public void TransformSegments_WithNullTransformation_ThrowsException()
        {
            Func<string> action = () => "TestValue".TransformSegments(null);
            action.Should().ThrowExactly<ArgumentNullException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void TransformSegments_WithNullOrEmptySeparator_ThrowsException(string separator)
        {
            Func<string> action = () => "TestValue".TransformSegments(s => s, separator);
            action.Should().ThrowExactly<ArgumentException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void TransformSegments_WithNullOrEmptyValue_ReturnsEmptyString(string value)
        {
            var result = value.TransformSegments(s => s);
            result.Should().BeEmpty();
        }

        [Fact]
        public void TransformSegments_WithSingleSegment_ReturnsTransformedSegemnt()
        {
            var result = "testseg1".TransformSegments(s => $"{s}-transformed");

            result.Should().Be("testseg1-transformed");
        }

        [Fact]
        public void TransformSegments_WithMultipleSegments_ReturnsTransformedSegemnts()
        {
            var result = "testseg1 testseg2 testseg3".TransformSegments(s => $"{s}-transformed");

            result.Should().Be("testseg1-transformed testseg2-transformed testseg3-transformed");
        }

        [Fact]
        public void TransformSegments_WithMultipleSegmentsAndWhiteSpace_ReturnsTransformedSegemnts()
        {
            var result = "testseg1     testseg2         testseg3".TransformSegments(s => $"{s}-transformed");

            result.Should().Be("testseg1-transformed testseg2-transformed testseg3-transformed");
        }
    }
}