using System;
using Dfc.CourseDirectory.FindAnApprenticeshipApi.Helper;
using Xunit;

namespace Dfc.CourseDirectory.FindAnApprenticeship.Tests.Helper
{
    public class StringHelperTests : IDisposable
    {
        public StringHelperTests()
        {

        }

        public class EnforceSpacesAfterFullstops
        {
            public class WhereSpaceIsNotDetected
            {
                string _textToProcess = "Content with no spaces between full stops.This should be spaced correctly. As should this...";

                [Fact]
                public void SpaceIsCorrectlyEnforced()
                {
                    // arrange
                    var expected = "Content with no spaces between full stops. This should be spaced correctly. As should this...";

                    // act
                    var actual = _textToProcess.EnforceSpacesAfterFullstops();

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
