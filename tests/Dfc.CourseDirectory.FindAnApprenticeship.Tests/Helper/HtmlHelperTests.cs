using System;
using System.Threading.Tasks;
using Xunit;
using Dfc.Providerportal.FindAnApprenticeship.Helper;

namespace Dfc.ProviderPortal.FindAnApprenticeship.UnitTests.Helper
{
    public class HtmlHelperTests : IDisposable
    {
        public HtmlHelperTests()
        {

        }

        public class StripHtmlTags
        {
            private static string _htmlContent =
                "<p>some html content with <strong>formatted</strong> text.</p>" +
                "<p>Second paragraph.This para has a line break <br /><blockquote>with a quote</blockquote><br /> in the middle</p>";

            [Fact]
            public void ShouldRemoveAllHtmlTags()
            {
                // arrange
                var expected = "some html content with formatted text. Second paragraph. This para has a line break with a quote in the middle";

                // act
                var actual = HtmlHelper.StripHtmlTags(_htmlContent);

                // assert
                Assert.Equal(expected, actual);
            }

            public class WhenPreserveLineBreaksIsTrue
            {
                [Fact]
                public void LineBreaksShouldBePreserved()
                {
                    // arrange
                    var expected = "some html content with formatted text.\r\n\r\nSecond paragraph. This para has a line break \r\n\r\nwith a quote\r\n\r\n in the middle";

                    // act
                    var actual = HtmlHelper.StripHtmlTags(_htmlContent, true);

                    // assert
                    Assert.Equal(expected, actual);
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
