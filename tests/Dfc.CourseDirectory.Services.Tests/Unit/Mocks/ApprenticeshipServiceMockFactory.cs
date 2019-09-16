using System;
using System.Collections.Generic;
using System.Text;
using Dfc.CourseDirectory.Services.Interfaces.ApprenticeshipService;
using Moq;

namespace Dfc.CourseDirectory.Services.Tests.Unit.Mocks
{
    public static class ApprenticeshipServiceMockFactory
    {
        public static IApprenticeshipService GetApprenticeshipService()
        {
            var mock = new Mock<IApprenticeshipService>();
            return mock.Object;

        }
    }
}
