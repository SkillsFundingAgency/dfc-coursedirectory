using System.Collections.Generic;
using Dfc.ProviderPortal.FindACourse.Helpers.Faoc;
using Xunit;

namespace Dfc.ProviderPortal.FindACourse.Tests
{
    public class OnlineCourseSearchServiceWrapperTests
    {
        [Theory]
        [MemberData(nameof(TranslateCourseSearchSubjectTextData))]
        public void TranslateCourseSearchSubjectText(string input, string expectedOutput)
        {
            var output = OnlineCourseSearchServiceWrapper.TranslateCourseSearchSubjectText(input);
            Assert.Equal(expectedOutput, output);
        }

        public static IEnumerable<object[]> TranslateCourseSearchSubjectTextData()
        {
            // Empty input should be mapped to wildcard
            yield return new object[] { (string)null, "*" };
            yield return new object[] { "", "*" };
            yield return new object[] { " ", "*" };
            yield return new object[] { "   ", "*" };

            // Input is trimmed
            yield return new object[] { " foo  ", "foo*" };

            // Terms are logically ORed
            yield return new object[] { "foo bar", "foo* | bar*" };

            // Add wildcard to end of each word
            yield return new object[] { "foo bar", "foo* | bar*" };

            // Terms in single quotes should not be prefix searches
            yield return new object[] { "'foo'", "(foo)" };

            // Single quote grouping
            yield return new object[] { "'foo' 'bar baz'", "(foo) | (bar + baz)" };

            // Double quotes
            yield return new object[] { "\"foo\"", "(\"foo\")" };
            yield return new object[] { "\"foo bar\"", "(\"foo bar\")" };

            // Escapes special characters
            yield return new object[] { "'foo+bar'", "(foo\\+bar)" };
            yield return new object[] { "'foo|bar'", "(foo\\|bar)" };
            yield return new object[] { "'foo-bar'", "(foo\\-bar)" };
            yield return new object[] { "'foo*bar'", "(foo\\*bar)" };
            yield return new object[] { "'foo(bar'", "(foo\\(bar)" };
            yield return new object[] { "'foo)bar'", "(foo\\)bar)" };

            // Combinations...
            yield return new object[] { "foo 'bar baz' \"qux qu|ux\"", "(bar + baz) | (\"qux qu\\|ux\") | foo*" };
        }
    }
}