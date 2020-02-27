using Dfc.CourseDirectory.WebV2.Validation;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests
{
    public class HtmlSanitizerTests
    {
        [Fact]
        public void AllTagsInWhitelist_ReturnsIdenticalHtml()
        {
            // Arrange
            var html = "<div>foo <h1>bar</h1> <p>baz<b>qux</b></p></div>";

            // Act
            var sanitized = HtmlSanitizer.SanitizeHtml(html);

            // Assert
            Assert.Equal(html, sanitized);
        }

        [Fact]
        public void ForbiddenTagsAreRemoved_ReturnsTrue()
        {
            // Arrange
            var html = "<p>foo<script>alert('bad things')</script></p>";

            // Act
            var sanitized = HtmlSanitizer.SanitizeHtml(html);

            // Assert
            Assert.Equal("<p>foo</p>", sanitized);
        }
    }
}
