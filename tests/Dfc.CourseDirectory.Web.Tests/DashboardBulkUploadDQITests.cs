using Dfc.CourseDirectory.Services.Interfaces.ApprenticeshipService;
using Dfc.CourseDirectory.Services.Interfaces.BlobStorageService;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.ProviderService;
using Dfc.CourseDirectory.Services.Interfaces.VenueService;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.Web.Tests.IntegrationHelpers;
using Dfc.CourseDirectory.Web.ViewComponents.Dashboard;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Dfc.CourseDirectory.Web.Tests
{
    /// <summary>
    /// Integration tests to get to the bottom of dashboard DQI issues with bulk upload. COUR-1930
    /// </summary>
    public class DashboardBulkUploadDQITests
    {
        public class MockHttpSession : ISession
        {
            Dictionary<string, object> sessionStorage = new Dictionary<string, object>();

            public object this[string name]
            {
                get { return sessionStorage[name]; }
                set { sessionStorage[name] = value; }
            }

            string ISession.Id
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            bool ISession.IsAvailable
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            IEnumerable<string> ISession.Keys
            {
                get { return sessionStorage.Keys; }
            }

            void ISession.Clear()
            {
                sessionStorage.Clear();
            }

            Task ISession.CommitAsync(CancellationToken token)
            {
                throw new NotImplementedException();
            }

            Task ISession.LoadAsync(CancellationToken token)
            {
                throw new NotImplementedException();
            }

            void ISession.Remove(string key)
            {
                sessionStorage.Remove(key);
            }

            void ISession.Set(string key, byte[] value)
            {
                sessionStorage[key] = Encoding.Unicode.GetString(value);
            }

            bool ISession.TryGetValue(string key, out byte[] value)
            {
                if (sessionStorage[key] != null)
                {
                    value = Encoding.Unicode.GetBytes(sessionStorage[key].ToString());
                    return true;
                }
                else
                {
                    value = null;
                    return false;
                }
            }
        }

        // Seeing if we can integration test the dashboard Bulk Upload DQIs
        [Fact]
        public void tdd()
        {
            // Arrange

            var mockSession = new MockHttpSession();
            mockSession.SetInt32("UKPRN", 10000712);

            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var context = new DefaultHttpContext();
            context.Session = mockSession;
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);

            ICourseService courseService = CourseServiceTestFactory.GetService();
            IVenueService venueService = VenueServiceTestFactory.GetService();
            IHttpContextAccessor contextAccessor = mockHttpContextAccessor.Object;
            IBlobStorageService blobStorageService = BlobStorageServiceTestFactory.GetService();
            IApprenticeshipService apprenticeshipService = ApprenticeshipServiceTestFactory.GetService();
            IProviderService providerService = ProviderServiceTestFactory.GetService();
            IEnvironmentHelper environmentHelper = EnvironmentHelperTestFactory.GetService();

            // Act

            var dashboardViewComponent = new Dashboard(courseService, venueService, contextAccessor, blobStorageService, apprenticeshipService, providerService, environmentHelper);
            var vcr = dashboardViewComponent.InvokeAsync(null).Result;
            var vvcr = vcr as Microsoft.AspNetCore.Mvc.ViewComponents.ViewViewComponentResult;
            var model = vvcr.ViewData.Model as DashboardModel;

            // Assert

            model.BulkUploadTotalCount.Should().Be(0);
        }
    }
}
