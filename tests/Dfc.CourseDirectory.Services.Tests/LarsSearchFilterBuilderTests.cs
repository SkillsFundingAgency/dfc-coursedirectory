using Xunit;

namespace Dfc.CourseDirectory.Services.Tests
{
    public class LarsSearchFilterBuilderTests
    {
        [Fact]
        public void Field_NotionalNVQLevelv2_eq_4_and_Field_AwardOrgCode_eq_NONE()
        {
            // arrange
            var expected = "NotionalNVQLevelv2 eq '4' and AwardOrgCode eq 'NONE'";

            // act
            var actual = new LarsSearchFilterBuilder()
                .Field("NotionalNVQLevelv2")
                .EqualTo("4")
                .And()
                .Field("AwardOrgCode")
                .EqualTo("NONE")
                .Build();

            // assert
            Assert.Equal(expected, actual);
        }
    }
}