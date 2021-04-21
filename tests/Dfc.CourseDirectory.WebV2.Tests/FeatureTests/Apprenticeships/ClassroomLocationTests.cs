using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Testing;
using Dfc.CourseDirectory.WebV2.Features.Apprenticeships.ClassroomLocation;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.Apprenticeships
{
    public class ClassroomLocationTests : MvcTestBase
    {
        public ClassroomLocationTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task Get_ValidRequest_RendersExpectedOutput()
        {
            // Arrange
            var provider = await TestData.CreateProvider(providerType: ProviderType.Apprenticeships);

            await TestData.CreateVenue(provider.ProviderId, venueName: "Venue 1");
            await TestData.CreateVenue(provider.ProviderId, venueName: "Venue 2");

            var parentMptxInstance = MptxManager.CreateInstance(new ParentFlow());
            var childMptxInstance = MptxManager.CreateInstance<FlowModel, IFlowModelCallback>(
                parentMptxInstance.InstanceId,
                FlowModel.Add(provider.ProviderId, cancelable: true),
                new Dictionary<string, object>()
                {
                    { "ReturnUrl", "callback" }
                });

            await User.AsProviderUser(provider.ProviderId, ProviderType.Apprenticeships);

            // Act
            var response = await HttpClient.GetAsync(
                $"/apprenticeships/classroom-location?ffiid={childMptxInstance.InstanceId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();

            var venues = doc.GetElementById("VenueId").GetElementsByTagName("option");
            Assert.Equal(3, venues.Length);
            Assert.Equal("Venue 1", venues[1].TextContent.Trim());
            Assert.Equal("Venue 2", venues[2].TextContent.Trim());
        }

        [Fact]
        public async Task Post_MissingVenueId_RendersErrorMessage()
        {
            // Arrange
            var provider = await TestData.CreateProvider(providerType: ProviderType.Apprenticeships);

            var parentMptxInstance = MptxManager.CreateInstance(new ParentFlow());
            var childMptxInstance = MptxManager.CreateInstance<FlowModel, IFlowModelCallback>(
                parentMptxInstance.InstanceId,
                FlowModel.Add(provider.ProviderId, cancelable: true),
                new Dictionary<string, object>()
                {
                    { "ReturnUrl", "callback" }
                });

            await User.AsProviderUser(provider.ProviderId, ProviderType.Apprenticeships);

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("Radius", 15)
                .Add("DeliveryModes", ApprenticeshipDeliveryMode.DayRelease)
                .Add("DeliveryModes", ApprenticeshipDeliveryMode.BlockRelease)
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"/apprenticeships/classroom-location?ffiid={childMptxInstance.InstanceId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var doc = await response.GetDocument();
            doc.AssertHasError("VenueId", "Select the venue");
        }

        [Fact]
        public async Task Post_InvalidVenueId_RendersErrorMessage()
        {
            // Arrange
            var provider = await TestData.CreateProvider(providerType: ProviderType.Apprenticeships);

            var invalidVenueId = Guid.NewGuid();

            var parentMptxInstance = MptxManager.CreateInstance(new ParentFlow());
            var childMptxInstance = MptxManager.CreateInstance<FlowModel, IFlowModelCallback>(
                parentMptxInstance.InstanceId,
                FlowModel.Add(provider.ProviderId, cancelable: true),
                new Dictionary<string, object>()
                {
                    { "ReturnUrl", "callback" }
                });

            await User.AsProviderUser(provider.ProviderId, ProviderType.Apprenticeships);

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("VenueId", invalidVenueId)
                .Add("Radius", 15)
                .Add("DeliveryModes", ApprenticeshipDeliveryMode.DayRelease)
                .Add("DeliveryModes", ApprenticeshipDeliveryMode.BlockRelease)
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"/apprenticeships/classroom-location?ffiid={childMptxInstance.InstanceId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var doc = await response.GetDocument();
            doc.AssertHasError("VenueId", "Select the venue");
        }

        [Fact]
        public async Task Post_BlockedVenueId_RendersErrorMessage()
        {
            // Arrange
            var provider = await TestData.CreateProvider(providerType: ProviderType.Apprenticeships);

            var venueId = Guid.NewGuid();

            var parentMptxInstance = MptxManager.CreateInstance(
                new ParentFlow()
                {
                    BlockedVenueIds = new[] { venueId }
                });
            var childMptxInstance = MptxManager.CreateInstance<FlowModel, IFlowModelCallback>(
                parentMptxInstance.InstanceId,
                FlowModel.Add(provider.ProviderId, cancelable: true),
                new Dictionary<string, object>()
                {
                    { "ReturnUrl", "callback" }
                });

            await User.AsProviderUser(provider.ProviderId, ProviderType.Apprenticeships);

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("VenueId", venueId)
                .Add("Radius", 15)
                .Add("DeliveryModes", ApprenticeshipDeliveryMode.DayRelease)
                .Add("DeliveryModes", ApprenticeshipDeliveryMode.BlockRelease)
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"/apprenticeships/classroom-location?ffiid={childMptxInstance.InstanceId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var doc = await response.GetDocument();
            doc.AssertHasError("VenueId", "Select the venue");
        }

        [Fact]
        public async Task Post_NotNationalMissingRadius_RendersErrorMessage()
        {
            // Arrange
            var provider = await TestData.CreateProvider(providerType: ProviderType.Apprenticeships);

            var venueId = (await TestData.CreateVenue(provider.ProviderId)).Id;

            var parentMptxInstance = MptxManager.CreateInstance(new ParentFlow());
            var childMptxInstance = MptxManager.CreateInstance<FlowModel, IFlowModelCallback>(
                parentMptxInstance.InstanceId,
                FlowModel.Add(provider.ProviderId, cancelable: true),
                new Dictionary<string, object>()
                {
                    { "ReturnUrl", "callback" }
                });

            await User.AsProviderUser(provider.ProviderId, ProviderType.Apprenticeships);

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("VenueId", venueId)
                .Add("DeliveryModes", ApprenticeshipDeliveryMode.DayRelease)
                .Add("DeliveryModes", ApprenticeshipDeliveryMode.BlockRelease)
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"/apprenticeships/classroom-location?ffiid={childMptxInstance.InstanceId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var doc = await response.GetDocument();
            doc.AssertHasError("Radius", "Enter how far you are willing to travel from the selected venue");
        }

        [Fact]
        public async Task Post_MissingDeliveryModes_RendersErrorMessage()
        {
            // Arrange
            var provider = await TestData.CreateProvider(providerType: ProviderType.Apprenticeships);

            var venueId = (await TestData.CreateVenue(provider.ProviderId)).Id;

            var parentMptxInstance = MptxManager.CreateInstance(new ParentFlow());
            var childMptxInstance = MptxManager.CreateInstance<FlowModel, IFlowModelCallback>(
                parentMptxInstance.InstanceId,
                FlowModel.Add(provider.ProviderId, cancelable: true),
                new Dictionary<string, object>()
                {
                    { "ReturnUrl", "callback" }
                });

            await User.AsProviderUser(provider.ProviderId, ProviderType.Apprenticeships);

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("VenueId", venueId)
                .Add("Radius", 15)
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"/apprenticeships/classroom-location?ffiid={childMptxInstance.InstanceId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var doc = await response.GetDocument();
            doc.AssertHasError("DeliveryModes", "Select at least one option from Day Release and Block Release");
        }

        [Fact]
        public async Task Post_ValidRequest_UpdatesParentStateAndRedirects()
        {
            // Arrange
            var provider = await TestData.CreateProvider(providerType: ProviderType.Apprenticeships);

            var venueId = (await TestData.CreateVenue(provider.ProviderId)).Id;

            var parentMptxInstance = MptxManager.CreateInstance(new ParentFlow());
            var childMptxInstance = MptxManager.CreateInstance<FlowModel, IFlowModelCallback>(
                parentMptxInstance.InstanceId,
                FlowModel.Add(provider.ProviderId, cancelable: true),
                new Dictionary<string, object>()
                {
                    { "ReturnUrl", "callback" }
                });

            await User.AsProviderUser(provider.ProviderId, ProviderType.Apprenticeships);

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("VenueId", venueId)
                .Add("Radius", 15)
                .Add("DeliveryModes", ApprenticeshipDeliveryMode.DayRelease)
                .Add("DeliveryModes", ApprenticeshipDeliveryMode.BlockRelease)
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"/apprenticeships/classroom-location?ffiid={childMptxInstance.InstanceId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.Found, response.StatusCode);
            Assert.Equal("callback", response.Headers.Location.OriginalString);

            Assert.Equal(venueId, parentMptxInstance.State.VenueId);
            Assert.Equal(15, parentMptxInstance.State.Radius);
            Assert.Contains(ApprenticeshipDeliveryMode.BlockRelease, parentMptxInstance.State.DeliveryModes);
            Assert.Contains(ApprenticeshipDeliveryMode.DayRelease, parentMptxInstance.State.DeliveryModes);
        }

        [Fact]
        public async Task GetRemove_ModeNotEdit_ReturnsBadRequest()
        {
            // Arrange
            var provider = await TestData.CreateProvider(providerType: ProviderType.Apprenticeships);

            await TestData.CreateVenue(provider.ProviderId, venueName: "The Venue");

            var parentMptxInstance = MptxManager.CreateInstance(new ParentFlow());
            var childMptxInstance = MptxManager.CreateInstance<FlowModel, IFlowModelCallback>(
                parentMptxInstance.InstanceId,
                FlowModel.Add(provider.ProviderId, cancelable: true),
                new Dictionary<string, object>()
                {
                    { "ReturnUrl", "callback" }
                });

            await User.AsProviderUser(provider.ProviderId, ProviderType.Apprenticeships);

            // Act
            var response = await HttpClient.GetAsync(
                $"apprenticeships/remove-classroom-location?ffiid={childMptxInstance.InstanceId}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetRemove_ValidRequest_ReturnsExpectedContent()
        {
            // Arrange
            var provider = await TestData.CreateProvider(providerType: ProviderType.Apprenticeships);

            var venueId = (await TestData.CreateVenue(provider.ProviderId, venueName: "The Venue")).Id;

            var parentMptxInstance = MptxManager.CreateInstance(new ParentFlow());
            var childMptxInstance = MptxManager.CreateInstance<FlowModel, IFlowModelCallback>(
                parentMptxInstance.InstanceId,
                FlowModel.Edit(
                    provider.ProviderId,
                    venueId,
                    radius: 5,
                    new[] { ApprenticeshipDeliveryMode.BlockRelease }),
                new Dictionary<string, object>()
                {
                    { "ReturnUrl", "callback" }
                });

            await User.AsProviderUser(provider.ProviderId, ProviderType.Apprenticeships);

            // Act
            var response = await HttpClient.GetAsync(
                $"apprenticeships/remove-classroom-location?ffiid={childMptxInstance.InstanceId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = await response.GetDocument();
            Assert.Equal("The Venue", doc.GetElementsByTagName("h2").First().TextContent);
        }

        [Fact]
        public async Task PostRemove_ModeNotEdit_ReturnsBadRequest()
        {
            // Arrange
            var provider = await TestData.CreateProvider(providerType: ProviderType.Apprenticeships);

            var parentMptxInstance = MptxManager.CreateInstance(new ParentFlow());
            var childMptxInstance = MptxManager.CreateInstance<FlowModel, IFlowModelCallback>(
                parentMptxInstance.InstanceId,
                FlowModel.Add(provider.ProviderId, cancelable: true),
                new Dictionary<string, object>()
                {
                    { "ReturnUrl", "callback" }
                });

            await User.AsProviderUser(provider.ProviderId, ProviderType.Apprenticeships);

            var requestContent = new FormUrlEncodedContentBuilder().ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"apprenticeships/remove-classroom-location?ffiid={childMptxInstance.InstanceId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PostRemove_ValidRequest_UpdatesParentStateAndRedirects()
        {
            // Arrange
            var provider = await TestData.CreateProvider(providerType: ProviderType.Apprenticeships);

            var venueId = (await TestData.CreateVenue(provider.ProviderId)).Id;

            var parentMptxInstance = MptxManager.CreateInstance(new ParentFlow());
            var childMptxInstance = MptxManager.CreateInstance<FlowModel, IFlowModelCallback>(
                parentMptxInstance.InstanceId,
                FlowModel.Edit(
                    provider.ProviderId,
                    venueId,
                    radius: 5,
                    new[] { ApprenticeshipDeliveryMode.BlockRelease }),
                new Dictionary<string, object>()
                {
                    { "ReturnUrl", "callback" }
                });

            await User.AsProviderUser(provider.ProviderId, ProviderType.Apprenticeships);

            var requestContent = new FormUrlEncodedContentBuilder().ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"apprenticeships/remove-classroom-location?ffiid={childMptxInstance.InstanceId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.Found, response.StatusCode);
            Assert.Equal("callback", response.Headers.Location.OriginalString);

            Assert.Null(parentMptxInstance.State.VenueId);
            Assert.Null(parentMptxInstance.State.Radius);
            Assert.Null(parentMptxInstance.State.DeliveryModes);
        }

        private class ParentFlow : IFlowModelCallback
        {
            public Guid? VenueId { get; set; }
            public Guid? OriginalVenueId { get; set; }
            public int? Radius { get; set; }
            public IEnumerable<ApprenticeshipDeliveryMode> DeliveryModes { get; set; }

            public IReadOnlyCollection<Guid> BlockedVenueIds { get; set; }

            public void ReceiveLocation(
                string instanceId,
                Guid venueId,
                Guid? originalVenueId,
                int radius,
                IEnumerable<ApprenticeshipDeliveryMode> deliveryModes)
            {
                VenueId = venueId;
                OriginalVenueId = originalVenueId;
                Radius = radius;
                DeliveryModes = deliveryModes;
            }

            public void RemoveLocation(Guid venueId)
            {
                VenueId = null;
                OriginalVenueId = null;
                Radius = null;
                DeliveryModes = null;
            }
        }
    }
}
