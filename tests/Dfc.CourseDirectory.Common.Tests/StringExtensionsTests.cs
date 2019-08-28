using FluentAssertions;
using Xunit;

namespace Dfc.CourseDirectory.Common.Tests
{
    public class StringExtensionsTests
    {
        [Fact]
        public void When_StringContainsNoSentences_Then_ReturnEntireString()
        {
            string sut = "The quick brown fox jumped over the lazy dogs";

            var result = sut.FirstSentence();

            result.Should().Be("The quick brown fox jumped over the lazy dogs");
        }

        [Fact]
        public void When_StringContainsOneSentence_Then_ReturnFirstSentance()
        {
            string sut = "The quick brown fox jumped over the lazy dogs.";

            var result = sut.FirstSentence();

            result.Should().Be("The quick brown fox jumped over the lazy dogs.");
        }

        [Fact]
        public void When_StringContainsMultipleSentences_Then_ReturnFirstSentence()
        {
            string sut = "The brown fox was quick. It jumped over dogs. The dogs were lazy.";

            var result = sut.FirstSentence();

            result.Should().Be("The brown fox was quick.");
        }
    }
}
