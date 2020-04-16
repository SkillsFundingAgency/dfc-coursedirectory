using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Features.Apprenticeships.ClassroomLocation;
using Dfc.CourseDirectory.WebV2.Models;
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
            var providerId = await TestData.CreateProvider(providerType: ProviderType.Apprenticeships);

            await TestData.CreateVenue(providerId, venueName: "Venue 1");
            await TestData.CreateVenue(providerId, venueName: "Venue 2");

            var parentMptxInstance = MptxManager.CreateInstance(new ParentFlow());
            var childMptxInstance = MptxManager.CreateInstance<FlowModel, IFlowModelCallback>(
                parentMptxInstance.InstanceId,
                FlowModel.Add(providerId),
                new Dictionary<string, object>()
                {
                    { "ReturnUrl", "callback" }
                });

            await User.AsProviderUser(providerId, ProviderType.Apprenticeships);

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
            var providerId = await TestData.CreateProvider(providerType: ProviderType.Apprenticeships);

            var parentMptxInstance = MptxManager.CreateInstance(new ParentFlow());
            var childMptxInstance = MptxManager.CreateInstance<FlowModel, IFlowModelCallback>(
                parentMptxInstance.InstanceId,
                FlowModel.Add(providerId),
                new Dictionary<string, object>()
                {
                    { "ReturnUrl", "callback" }
                });

            await User.AsProviderUser(providerId, ProviderType.Apprenticeships);

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("Radius", 15)
                .Add("National", false)
                .Add("DeliveryModes", ApprenticeshipDeliveryModes.DayRelease)
                .Add("DeliveryModes", ApprenticeshipDeliveryModes.BlockRelease)
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"/apprenticeships/classroom-location?ffiid={childMptxInstance.InstanceId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var doc = await response.GetDocument();
            doc.AssertHasError("VenueId", "Select the location");
        }

        [Fact]
        public async Task Post_InvalidVenueId_RendersErrorMessage()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(providerType: ProviderType.Apprenticeships);

            var invalidVenueId = Guid.NewGuid();

            var parentMptxInstance = MptxManager.CreateInstance(new ParentFlow());
            var childMptxInstance = MptxManager.CreateInstance<FlowModel, IFlowModelCallback>(
                parentMptxInstance.InstanceId,
                FlowModel.Add(providerId),
                new Dictionary<string, object>()
                {
                    { "ReturnUrl", "callback" }
                });

            await User.AsProviderUser(providerId, ProviderType.Apprenticeships);

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("VenueId", invalidVenueId)
                .Add("Radius", 15)
                .Add("National", false)
                .Add("DeliveryModes", ApprenticeshipDeliveryModes.DayRelease)
                .Add("DeliveryModes", ApprenticeshipDeliveryModes.BlockRelease)
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"/apprenticeships/classroom-location?ffiid={childMptxInstance.InstanceId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var doc = await response.GetDocument();
            doc.AssertHasError("VenueId", "Select the location");
        }

        [Fact]
        public async Task Post_BlockedVenueId_RendersErrorMessage()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(providerType: ProviderType.Apprenticeships);

            var venueId = Guid.NewGuid();

            var parentMptxInstance = MptxManager.CreateInstance(
                new ParentFlow()
                {
                    BlockedVenueIds = new[] { venueId }
                });
            var childMptxInstance = MptxManager.CreateInstance<FlowModel, IFlowModelCallback>(
                parentMptxInstance.InstanceId,
                FlowModel.Add(providerId),
                new Dictionary<string, object>()
                {
                    { "ReturnUrl", "callback" }
                });

            await User.AsProviderUser(providerId, ProviderType.Apprenticeships);

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("VenueId", venueId)
                .Add("Radius", 15)
                .Add("National", false)
                .Add("DeliveryModes", ApprenticeshipDeliveryModes.DayRelease)
                .Add("DeliveryModes", ApprenticeshipDeliveryModes.BlockRelease)
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"/apprenticeships/classroom-location?ffiid={childMptxInstance.InstanceId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var doc = await response.GetDocument();
            doc.AssertHasError("VenueId", "Select the location");
        }

        [Fact]
        public async Task Post_NotNationalMissingRadius_RendersErrorMessage()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(providerType: ProviderType.Apprenticeships);

            var venueId = await TestData.CreateVenue(providerId);

            var parentMptxInstance = MptxManager.CreateInstance(new ParentFlow());
            var childMptxInstance = MptxManager.CreateInstance<FlowModel, IFlowModelCallback>(
                parentMptxInstance.InstanceId,
                FlowModel.Add(providerId),
                new Dictionary<string, object>()
                {
                    { "ReturnUrl", "callback" }
                });

            await User.AsProviderUser(providerId, ProviderType.Apprenticeships);

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("VenueId", venueId)
                .Add("National", false)
                .Add("DeliveryModes", ApprenticeshipDeliveryModes.DayRelease)
                .Add("DeliveryModes", ApprenticeshipDeliveryModes.BlockRelease)
                .ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"/apprenticeships/classroom-location?ffiid={childMptxInstance.InstanceId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var doc = await response.GetDocument();
            doc.AssertHasError("Radius", "Enter how far you are willing to travel from the selected location");
        }

        [Fact]
        public async Task Post_MissingDeliveryModes_RendersErrorMessage()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(providerType: ProviderType.Apprenticeships);

            var venueId = await TestData.CreateVenue(providerId);

            var parentMptxInstance = MptxManager.CreateInstance(new ParentFlow());
            var childMptxInstance = MptxManager.CreateInstance<FlowModel, IFlowModelCallback>(
                parentMptxInstance.InstanceId,
                FlowModel.Add(providerId),
                new Dictionary<string, object>()
                {
                    { "ReturnUrl", "callback" }
                });

            await User.AsProviderUser(providerId, ProviderType.Apprenticeships);

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("VenueId", venueId)
                .Add("Radius", 15)
                .Add("National", false)
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
            var providerId = await TestData.CreateProvider(providerType: ProviderType.Apprenticeships);

            var venueId = await TestData.CreateVenue(providerId);

            var parentMptxInstance = MptxManager.CreateInstance(new ParentFlow());
            var childMptxInstance = MptxManager.CreateInstance<FlowModel, IFlowModelCallback>(
                parentMptxInstance.InstanceId,
                FlowModel.Add(providerId),
                new Dictionary<string, object>()
                {
                    { "ReturnUrl", "callback" }
                });

            await User.AsProviderUser(providerId, ProviderType.Apprenticeships);

            var requestContent = new FormUrlEncodedContentBuilder()
                .Add("VenueId", venueId)
                .Add("Radius", 15)
                .Add("National", false)
                .Add("DeliveryModes", ApprenticeshipDeliveryModes.DayRelease)
                .Add("DeliveryModes", ApprenticeshipDeliveryModes.BlockRelease)
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
            Assert.False(parentMptxInstance.State.National);
            Assert.Equal(
                ApprenticeshipDeliveryModes.BlockRelease | ApprenticeshipDeliveryModes.DayRelease,
                parentMptxInstance.State.DeliveryModes);
        }

        [Fact]
        public async Task GetRemove_ModeNotEdit_ReturnsBadRequest()
        {
            // Arrange
            var providerId = await TestData.CreateProvider(providerType: ProviderType.Apprenticeships);

            var venueId = await TestData.CreateVenue(providerId, venueName: "The Venue");

            var parentMptxInstance = MptxManager.CreateInstance(new ParentFlow());
            var childMptxInstance = MptxManager.CreateInstance<FlowModel, IFlowModelCallback>(
                parentMptxInstance.InstanceId,
                FlowModel.Add(providerId),
                new Dictionary<string, object>()
                {
                    { "ReturnUrl", "callback" }
                });

            await User.AsProviderUser(providerId, ProviderType.Apprenticeships);

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
            var providerId = await TestData.CreateProvider(providerType: ProviderType.Apprenticeships);

            var venueId = await TestData.CreateVenue(providerId, venueName: "The Venue");

            var parentMptxInstance = MptxManager.CreateInstance(new ParentFlow());
            var childMptxInstance = MptxManager.CreateInstance<FlowModel, IFlowModelCallback>(
                parentMptxInstance.InstanceId,
                FlowModel.Edit(
                    providerId,
                    venueId,
                    national: true,
                    radius: null,
                    ApprenticeshipDeliveryModes.BlockRelease),
                new Dictionary<string, object>()
                {
                    { "ReturnUrl", "callback" }
                });

            await User.AsProviderUser(providerId, ProviderType.Apprenticeships);

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
            var providerId = await TestData.CreateProvider(providerType: ProviderType.Apprenticeships);

            var parentMptxInstance = MptxManager.CreateInstance(new ParentFlow());
            var childMptxInstance = MptxManager.CreateInstance<FlowModel, IFlowModelCallback>(
                parentMptxInstance.InstanceId,
                FlowModel.Add(providerId),
                new Dictionary<string, object>()
                {
                    { "ReturnUrl", "callback" }
                });

            await User.AsProviderUser(providerId, ProviderType.Apprenticeships);

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
            var providerId = await TestData.CreateProvider(providerType: ProviderType.Apprenticeships);

            var venueId = await TestData.CreateVenue(providerId);

            var parentMptxInstance = MptxManager.CreateInstance(new ParentFlow());
            var childMptxInstance = MptxManager.CreateInstance<FlowModel, IFlowModelCallback>(
                parentMptxInstance.InstanceId,
                FlowModel.Edit(
                    providerId,
                    venueId,
                    national: true,
                    radius: null,
                    ApprenticeshipDeliveryModes.BlockRelease),
                new Dictionary<string, object>()
                {
                    { "ReturnUrl", "callback" }
                });

            await User.AsProviderUser(providerId, ProviderType.Apprenticeships);

            var requestContent = new FormUrlEncodedContentBuilder().ToContent();

            // Act
            var response = await HttpClient.PostAsync(
                $"apprenticeships/remove-classroom-location?ffiid={childMptxInstance.InstanceId}",
                requestContent);

            // Assert
            Assert.Equal(HttpStatusCode.Found, response.StatusCode);
            Assert.Equal("callback", response.Headers.Location.OriginalString);

            Assert.Null(parentMptxInstance.State.VenueId);
            Assert.Null(parentMptxInstance.State.National);
            Assert.Null(parentMptxInstance.State.Radius);
            Assert.Null(parentMptxInstance.State.DeliveryModes);
        }

        private class ParentFlow : IFlowModelCallback
        {
            public Guid? VenueId { get; set; }
            public Guid? OriginalVenueId { get; set; }
            public bool? National { get; set; }
            public int? Radius { get; set; }
            public ApprenticeshipDeliveryModes? DeliveryModes { get; set; }

            public IReadOnlyCollection<Guid> BlockedVenueIds { get; set; }

            public void ReceiveLocation(
                string instanceId,
                Guid venueId,
                Guid? originalVenueId,
                bool national,
                int? radius,
                ApprenticeshipDeliveryModes deliveryModes)
            {
                VenueId = venueId;
                OriginalVenueId = originalVenueId;
                National = national;
                Radius = radius;
                DeliveryModes = deliveryModes;
            }

            public void RemoveLocation(Guid venueId)
            {
                VenueId = null;
                OriginalVenueId = null;
                National = null;
                Radius = null;
                DeliveryModes = null;
            }
        }
    }
}
