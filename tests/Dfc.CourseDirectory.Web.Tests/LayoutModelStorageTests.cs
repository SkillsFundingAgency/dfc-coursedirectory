using System;
using System.Collections.Generic;
using Dfc.CourseDirectory.Web.Views.Shared;
using Xunit;

namespace Dfc.CourseDirectory.Web.Tests
{
    public class LayoutModelStorageTests
    {
        [Fact]
        public void LayoutModelRoundTrip()
        {
            var mockViewData = GetFakeViewData();
            var layoutModel = new LayoutModel { BackLink = true };

            mockViewData.SetLayoutModel(layoutModel);
            var retrievedModel = mockViewData.GetLayoutModel();

            Assert.Same(retrievedModel,layoutModel);
            Assert.Equal(retrievedModel.BackLink,layoutModel.BackLink);
        }

        [Fact]
        public void GetLayoutModelWhenNotSet()
        {
            var mockViewData = GetFakeViewData();
            var retrievedModel = mockViewData.GetLayoutModel();

            Assert.NotNull(retrievedModel);
        }

        [Fact]
        public void ThrowsForWrongTypeInViewData()
        {
            var mockViewData = GetFakeViewData();
            mockViewData[LayoutModelViedDataExtensions.ViewDataKey] = 666;
            Assert.Throws<InvalidCastException>(() =>
            {
                mockViewData.GetLayoutModel();
            });
        }

        private static IDictionary<string, object> GetFakeViewData()
        {
            return new Dictionary<string, object>();
        }
    }
}
