using Dfc.CourseDirectory.WebV2.Validation;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.ValidationTests
{
    public class HtmlTests
    {
        [Fact]
        public void SanitizeHtml_AllTagsInWhitelist_ReturnsIdenticalHtml()
        {
            // Arrange
            var html = "<div>foo <h1>bar</h1> <p>baz<b>qux</b></p></div>";

            // Act
            var sanitized = Html.SanitizeHtml(html);

            // Assert
            Assert.Equal(html, sanitized);
        }

        [Fact]
        public void SanitizeHtml_ForbiddenTagsAreRemoved_ReturnsTrue()
        {
            // Arrange
            var html = "<p>foo<script>alert('bad things')</script></p>";

            // Act
            var sanitized = Html.SanitizeHtml(html);

            // Assert
            Assert.Equal("<p>foo</p>", sanitized);
        }

        [Fact]
        public void StripTags_ReturnsExpectedValue()
        {
            // Arrange
            var html = "<p><b>foo <i>bar</i></b> <ul><li>baz</li></ul>";

            // Act
            var stripped = Html.StripTags(html);

            // Assert
            Assert.Equal("foo bar baz", stripped);
        }
    }
}
