using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.WebV2.Features.NewApprenticeshipProvider;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.NewApprenticeshipProvider
{
    public class FlowModelInitializerTests : DatabaseTestBase
    {
        public FlowModelInitializerTests(Testing.DatabaseTestBaseFixture factory) : base(factory)
        {
        }

        [Fact]
        public async Task Initialize_Populates_Apprenticeship_Fields_Standard()
        {
            // Arrange
            var Clock = Fixture.DatabaseFixture.Clock;
            var ukprn = 12347;
            var adminUserId = $"admin-user";
            var contactTelephone = "1111 111 1111";
            var contactWebsite = "https://somerandomprovider.com";
            var marketingInfo = "Providing Online training";
            var regions = new List<string> { "123" };

            var providerId = await TestData.CreateProvider(
                ukprn: ukprn,
                providerName: "Provider 1",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Submitted);

            var providerUserId = $"{ukprn}-user";
            var user = await TestData.CreateUser(providerUserId, "somebody@provider1.com", "Provider 1", "Person", providerId);
            var adminUser = await TestData.CreateUser(adminUserId, "admin@provider.com", "admin", "admin", null);
            var standard = await TestData.CreateStandard(standardCode: 1234, version: 1, standardName: "Test Standard");

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId,
                standard,
                createdBy: user,
                contactEmail: adminUser.Email,
                contactTelephone: contactTelephone,
                contactWebsite: contactWebsite,
                marketingInformation: marketingInfo,
                locations:  new List<CreateApprenticeshipLocation> {
                    CreateApprenticeshipLocation.CreateRegions(regions)
                });

            // Create submission
            var submissionId = await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            var mockCache = new Mock<IStandardsAndFrameworksCache>();
            var stan = new Core.Models.Standard { CosmosId = Guid.NewGuid(), NotionalNVQLevelv2 = "Level 2", OtherBodyApprovalRequired=true, StandardCode=1, StandardName="test", Version=1 };
            mockCache.Setup(w => w.GetStandard(It.IsAny<int>(), It.IsAny<int>())).Returns(Task.FromResult(stan));


            await WithSqlQueryDispatcher(async dispatcher =>
            {
                //act
                var initializer = new FlowModelInitializer(Fixture.DatabaseFixture.CosmosDbQueryDispatcher.Object, dispatcher, mockCache.Object);
                var model = await initializer.Initialize(providerId);

                //assert
                Assert.Equal(adminUser.Email, model.ApprenticeshipContactEmail);
                Assert.Equal(contactTelephone, model.ApprenticeshipContactTelephone);
                Assert.Equal(contactWebsite, model.ApprenticeshipContactWebsite);
                Assert.Equal(apprenticeshipId, model.ApprenticeshipId);
                Assert.False(model.ApprenticeshipIsNational);
                Assert.Equal(ApprenticeshipLocationType.EmployerBased, model.ApprenticeshipLocationType);
                Assert.Equal(marketingInfo, model.ApprenticeshipMarketingInformation);
                Assert.Null(model.ApprenticeshipClassroomLocations);
                Assert.Collection(model.ApprenticeshipLocationSubRegionIds,
                    item1 =>
                    {
                        Assert.Equal(item1, regions.First());
                    });
                Assert.Equal(contactWebsite, model.ApprenticeshipWebsite);
                Assert.False(model.ApprenticeshipStandardOrFramework.IsFramework);
                Assert.True(model.ApprenticeshipStandardOrFramework.IsStandard);
            });


        }

        [Fact]
        public async Task Initialize_Populates_Apprenticeship_Fields_Framework()
        {
            // Arrange
            var Clock = Fixture.DatabaseFixture.Clock;
            var ukprn = 12346;
            var adminUserId = $"admin-user";
            var contactTelephone = "1111 111 1111";
            var contactWebsite = "https://somerandomprovider.com";
            var marketingInfo = "Providing Online training";
            var regions = new List<string> { "123" };

            var providerId = await TestData.CreateProvider(
                ukprn: ukprn,
                providerName: "Provider 1",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Submitted);

            var providerUserId = $"{ukprn}-user";
            var user = await TestData.CreateUser(providerUserId, "somebody@provider1.com", "Provider 1", "Person", providerId);
            var adminUser = await TestData.CreateUser(adminUserId, "admin@provider.com", "admin", "admin", null);
            var framework = await TestData.CreateFramework(1, 1, 1, "Test Framework");

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId,
                framework,
                createdBy: user,
                contactEmail: adminUser.Email,
                contactTelephone: contactTelephone,
                contactWebsite: contactWebsite,
                marketingInformation: marketingInfo,
                locations:  new List<CreateApprenticeshipLocation> {
                    CreateApprenticeshipLocation.CreateRegions(regions)
                });

            // Create submission
            var submissionId = await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            var mockCache = new Mock<IStandardsAndFrameworksCache>();
            mockCache.Setup(w => w.GetFramework(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).Returns(Task.FromResult(framework));


            await WithSqlQueryDispatcher(async dispatcher =>
            {
                var initializer = new FlowModelInitializer(Fixture.DatabaseFixture.CosmosDbQueryDispatcher.Object, dispatcher, mockCache.Object);

                //act
                var model = await initializer.Initialize(providerId);

                //assert
                Assert.Equal(adminUser.Email, model.ApprenticeshipContactEmail);
                Assert.Equal(contactTelephone, model.ApprenticeshipContactTelephone);
                Assert.Equal(contactWebsite, model.ApprenticeshipContactWebsite);
                Assert.Equal(apprenticeshipId, model.ApprenticeshipId);
                Assert.False(model.ApprenticeshipIsNational);
                Assert.Equal(ApprenticeshipLocationType.EmployerBased, model.ApprenticeshipLocationType);
                Assert.Equal(marketingInfo, model.ApprenticeshipMarketingInformation);
                Assert.Null(model.ApprenticeshipClassroomLocations);
                Assert.Collection(model.ApprenticeshipLocationSubRegionIds,
                    item1 =>
                    {
                        Assert.Equal(item1, regions.First());
                    });
                Assert.Equal(contactWebsite, model.ApprenticeshipWebsite);
                Assert.True(model.ApprenticeshipStandardOrFramework.IsFramework);
                Assert.False(model.ApprenticeshipStandardOrFramework.IsStandard);
            });
        }

        [Fact]
        public async Task Initialize_Populates_NationalApprenticeship_Fields()
        {
            // Arrange
            var Clock = Fixture.DatabaseFixture.Clock;
            var ukprn = 12346;
            var adminUserId = $"admin-user";
            var contactTelephone = "1111 111 1111";
            var contactWebsite = "https://somerandomprovider.com";
            var marketingInfo = "Providing Online training";
            var regions = new List<string> { "123" };

            var providerId = await TestData.CreateProvider(
                ukprn: ukprn,
                providerName: "Provider 1",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Submitted);

            var providerUserId = $"{ukprn}-user";
            var user = await TestData.CreateUser(providerUserId, "somebody@provider1.com", "Provider 1", "Person", providerId);
            var adminUser = await TestData.CreateUser(adminUserId, "admin@provider.com", "admin", "admin", null);
            var framework = await TestData.CreateFramework(1, 1, 1, "Test Framework");

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId,
                framework,
                createdBy: user,
                contactEmail: adminUser.Email,
                contactTelephone: contactTelephone,
                contactWebsite: contactWebsite,
                marketingInformation: marketingInfo,
                locations: new List<CreateApprenticeshipLocation> {
                    CreateApprenticeshipLocation.CreateNational()
                });

            // Create submission
            var submissionId = await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            var mockCache = new Mock<IStandardsAndFrameworksCache>();
            mockCache.Setup(w => w.GetFramework(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).Returns(Task.FromResult(framework));

            await WithSqlQueryDispatcher(async dispatcher =>
            {
                var initializer = new FlowModelInitializer(Fixture.DatabaseFixture.CosmosDbQueryDispatcher.Object, dispatcher, mockCache.Object);

                //act
                var model = await initializer.Initialize(providerId);

                //assert
                Assert.Equal(adminUser.Email, model.ApprenticeshipContactEmail);
                Assert.Equal(contactTelephone, model.ApprenticeshipContactTelephone);
                Assert.Equal(contactWebsite, model.ApprenticeshipContactWebsite);
                Assert.Equal(apprenticeshipId, model.ApprenticeshipId);
                Assert.True(model.ApprenticeshipIsNational);
                Assert.Equal(ApprenticeshipLocationType.EmployerBased, model.ApprenticeshipLocationType);
                Assert.Equal(marketingInfo, model.ApprenticeshipMarketingInformation);
                Assert.Null(model.ApprenticeshipClassroomLocations);
                Assert.Equal(contactWebsite, model.ApprenticeshipWebsite);
                Assert.True(model.ApprenticeshipStandardOrFramework.IsFramework);
                Assert.False(model.ApprenticeshipStandardOrFramework.IsStandard);
            });
        }

        [Fact]
        public async Task Initialize_Does_Not_Populate_Apprenticeship_Fields_For_NewProvider()
        {
            // Arrange
            var ukprn = 12345;
            var marketingInfo = "example marketing information";

            var providerId = await TestData.CreateProvider(
                ukprn: ukprn,
                providerName: "Provider 1",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Submitted,
                marketingInformation: marketingInfo);
            var mockCache = new Mock<IStandardsAndFrameworksCache>();

            await WithSqlQueryDispatcher(async dispatcher =>
            {
                var initializer = new FlowModelInitializer(Fixture.DatabaseFixture.CosmosDbQueryDispatcher.Object, dispatcher, mockCache.Object);

                //act
                var model = await initializer.Initialize(providerId);

                //assert
                Assert.Null(model.ApprenticeshipContactEmail);
                Assert.Null(model.ApprenticeshipContactTelephone);
                Assert.Null(model.ApprenticeshipContactWebsite);
                Assert.Null(model.ApprenticeshipId);
                Assert.Null(model.ApprenticeshipIsNational);
                Assert.Null(model.ApprenticeshipLocationType);
                Assert.Null(model.ApprenticeshipMarketingInformation);
                Assert.Null(model.ApprenticeshipClassroomLocations);
                Assert.Null(model.ApprenticeshipLocationSubRegionIds);
                Assert.Null(model.ApprenticeshipWebsite);
            });
        }

        [Fact]
        public async Task Initialize_ApprenticeshipType_Is_BothClassroomAndEmployer()
        {
            // Arrange
            var Clock = Fixture.DatabaseFixture.Clock;
            var ukprn = 12346;
            var adminUserId = $"admin-user";
            var contactTelephone = "1111 111 1111";
            var contactWebsite = "https://somerandomprovider.com";
            var marketingInfo = "Providing Online training";
            var regions = new List<string> { "123" };
            var venueId = Guid.NewGuid();
            var radius = 10;
            var deliveryMode = ApprenticeshipDeliveryModes.BlockRelease;
            var providerId = await TestData.CreateProvider(
                ukprn: ukprn,
                providerName: "Provider 1",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Submitted);

            var providerUserId = $"{ukprn}-user";
            var user = await TestData.CreateUser(providerUserId, "somebody@provider1.com", "Provider 1", "Person", providerId);
            var adminUser = await TestData.CreateUser(adminUserId, "admin@provider.com", "admin", "admin", null);
            var framework = await TestData.CreateFramework(1, 1, 1, "Test Framework");

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId,
                framework,
                createdBy: user,
                contactEmail: adminUser.Email,
                contactTelephone: contactTelephone,
                contactWebsite: contactWebsite,
                marketingInformation: marketingInfo,
                locations: new List<CreateApprenticeshipLocation> {
                    CreateApprenticeshipLocation.CreateNational(),
                    CreateApprenticeshipLocation.CreateFromVenue(new Venue { Id=venueId, VenueName="test" }, radius, deliveryMode, ApprenticeshipLocationType.ClassroomBased)
                }); 

            // Create submission
            var submissionId = await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            var mockCache = new Mock<IStandardsAndFrameworksCache>();
            mockCache.Setup(w => w.GetFramework(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).Returns(Task.FromResult(framework));


            await WithSqlQueryDispatcher(async dispatcher =>
            {
                var initializer = new FlowModelInitializer(Fixture.DatabaseFixture.CosmosDbQueryDispatcher.Object, dispatcher, mockCache.Object);

                //act
                var model = await initializer.Initialize(providerId);

                //assert
                Assert.Equal(adminUser.Email, model.ApprenticeshipContactEmail);
                Assert.Equal(contactTelephone, model.ApprenticeshipContactTelephone);
                Assert.Equal(contactWebsite, model.ApprenticeshipContactWebsite);
                Assert.Equal(apprenticeshipId, model.ApprenticeshipId);
                Assert.True(model.ApprenticeshipIsNational);
                Assert.Equal(ApprenticeshipLocationType.ClassroomBasedAndEmployerBased, model.ApprenticeshipLocationType);
                Assert.Equal(marketingInfo, model.ApprenticeshipMarketingInformation);
                Assert.Collection<FlowModel.ClassroomLocationEntry>(new List<FlowModel.ClassroomLocationEntry>(model.ApprenticeshipClassroomLocations.Values),
                location =>
                {
                    Assert.Equal(venueId, location.VenueId);
                    Assert.Equal(radius, location.Radius);
                    Assert.Equal(deliveryMode, location.DeliveryModes);
                });
                Assert.Equal(contactWebsite, model.ApprenticeshipWebsite);
                Assert.True(model.ApprenticeshipStandardOrFramework.IsFramework);
                Assert.False(model.ApprenticeshipStandardOrFramework.IsStandard);
            });
            
        }

        [Fact]
        public async Task Initialize_ApprenticeshipType_Is_EmployerBased()
        {
            // Arrange
            var Clock = Fixture.DatabaseFixture.Clock;
            var ukprn = 12346;
            var adminUserId = $"admin-user";
            var contactTelephone = "1111 111 1111";
            var contactWebsite = "https://somerandomprovider.com";
            var marketingInfo = "Providing Online training";

            var providerId = await TestData.CreateProvider(
                ukprn: ukprn,
                providerName: "Provider 1",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Submitted);

            var providerUserId = $"{ukprn}-user";
            var user = await TestData.CreateUser(providerUserId, "somebody@provider1.com", "Provider 1", "Person", providerId);
            var adminUser = await TestData.CreateUser(adminUserId, "admin@provider.com", "admin", "admin", null);
            var framework = await TestData.CreateFramework(1, 1, 1, "Test Framework");

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId,
                framework,
                createdBy: user,
                contactEmail: adminUser.Email,
                contactTelephone: contactTelephone,
                contactWebsite: contactWebsite,
                marketingInformation: marketingInfo,
                locations: new List<CreateApprenticeshipLocation> {
                    CreateApprenticeshipLocation.CreateNational()
                });

            // Create submission
            var submissionId = await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            var mockCache = new Mock<IStandardsAndFrameworksCache>();
            mockCache.Setup(w => w.GetFramework(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).Returns(Task.FromResult(framework));


            await WithSqlQueryDispatcher(async dispatcher =>
            {

                var initializer = new FlowModelInitializer(Fixture.DatabaseFixture.CosmosDbQueryDispatcher.Object, dispatcher, mockCache.Object);

                //act
                var model = await initializer.Initialize(providerId);

                //assert
                Assert.Equal(adminUser.Email, model.ApprenticeshipContactEmail);
                Assert.Equal(contactTelephone, model.ApprenticeshipContactTelephone);
                Assert.Equal(contactWebsite, model.ApprenticeshipContactWebsite);
                Assert.Equal(apprenticeshipId, model.ApprenticeshipId);
                Assert.True(model.ApprenticeshipIsNational);
                Assert.Equal(ApprenticeshipLocationType.EmployerBased, model.ApprenticeshipLocationType);
                Assert.Equal(marketingInfo, model.ApprenticeshipMarketingInformation);
                Assert.Null(model.ApprenticeshipClassroomLocations);
                Assert.Equal(contactWebsite, model.ApprenticeshipWebsite);
                Assert.True(model.ApprenticeshipStandardOrFramework.IsFramework);
                Assert.False(model.ApprenticeshipStandardOrFramework.IsStandard);
            });
        }
    }
}
