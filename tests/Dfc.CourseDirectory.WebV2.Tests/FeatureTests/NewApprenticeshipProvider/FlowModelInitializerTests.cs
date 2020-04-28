using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.WebV2.Features.NewApprenticeshipProvider;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.NewApprenticeshipProvider
{
    public class FlowModelInitializerTests : DatabaseTestBase
    {
        public FlowModelInitializerTests(Testing.DatabaseTestBaseFixture factory)
            : base(factory)
        {
        }

        [Fact]
        public async Task Initialize_ApprenticeshipWithStandard_PopulatesModelCorrectly()
        {
            // Arrange
            var ukprn = 12347;
            var adminUserId = $"admin-user";
            var contactTelephone = "1111 111 1111";
            var website = "https://somerandomprovider.com/apprenticeship";
            var contactWebsite = "https://somerandomprovider.com";
            var marketingInfo = "Providing Online training";
            var regions = new List<string> { "123" };
            var contactEmail = "somecontact@nonexistentprovider.com";

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
                contactEmail: contactEmail,
                contactTelephone: contactTelephone,
                contactWebsite: contactWebsite,
                marketingInformation: marketingInfo,
                website: website,
                locations: new[]
                {
                    CreateApprenticeshipLocation.CreateRegions(regions)
                });

            var standardsAndFrameworksCache = new StandardsAndFrameworksCache(CosmosDbQueryDispatcher.Object);

            var submissionId = await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            await WithSqlQueryDispatcher(async dispatcher =>
            {
                var initializer = new FlowModelInitializer(CosmosDbQueryDispatcher.Object, dispatcher, standardsAndFrameworksCache);

                // Act
                var model = await initializer.Initialize(providerId);

                // Assert
                Assert.True(model.GotApprenticeshipDetails);
                Assert.Equal(contactEmail, model.ApprenticeshipContactEmail);
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
                Assert.Equal(website, model.ApprenticeshipWebsite);
                Assert.False(model.ApprenticeshipStandardOrFramework.IsFramework);
                Assert.True(model.ApprenticeshipStandardOrFramework.IsStandard);
            });
        }

        [Fact]
        public async Task Initialize_ApprenticeshipWithFramework_PopulatesModelSuccessfully()
        {
            // Arrange
            var ukprn = 12346;
            var adminUserId = $"admin-user";
            var contactTelephone = "1111 111 1111";
            var website = "https://somerandomprovider.com/apprenticeship";
            var contactWebsite = "https://somerandomprovider.com";
            var marketingInfo = "Providing Online training";
            var regions = new List<string> { "123" };
            var contactEmail = "somecontact@nonexistentprovider.com";

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
                contactEmail: contactEmail,
                contactTelephone: contactTelephone,
                contactWebsite: contactWebsite,
                marketingInformation: marketingInfo,
                website: website,
                locations: new[]
                {
                    CreateApprenticeshipLocation.CreateRegions(regions)
                });

            var standardsAndFrameworksCache = new StandardsAndFrameworksCache(CosmosDbQueryDispatcher.Object);
            
            var submissionId = await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            await WithSqlQueryDispatcher(async dispatcher =>
            {
                var initializer = new FlowModelInitializer(CosmosDbQueryDispatcher.Object, dispatcher, standardsAndFrameworksCache);

                // Act
                var model = await initializer.Initialize(providerId);

                // Assert
                Assert.True(model.GotApprenticeshipDetails);
                Assert.Equal(contactEmail, model.ApprenticeshipContactEmail);
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
                Assert.Equal(website, model.ApprenticeshipWebsite);
                Assert.True(model.ApprenticeshipStandardOrFramework.IsFramework);
                Assert.False(model.ApprenticeshipStandardOrFramework.IsStandard);
            });
        }

        [Fact]
        public async Task Initialize_NationalApprenticeship_PopulatesModelCorrectly()
        {
            // Arrange
            var ukprn = 12346;
            var adminUserId = $"admin-user";
            var contactTelephone = "1111 111 1111";
            var website = "https://somerandomprovider.com/apprenticeship";
            var contactWebsite = "https://somerandomprovider.com";
            var marketingInfo = "Providing Online training";
            var regions = new List<string> { "123" };
            var contactEmail = "somecontact@nonexistentprovider.com";

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
                contactEmail: contactEmail,
                contactTelephone: contactTelephone,
                contactWebsite: contactWebsite,
                marketingInformation: marketingInfo,
                website: website,
                locations: new[]
                {
                    CreateApprenticeshipLocation.CreateNational()
                });

            var standardsAndFrameworksCache = new StandardsAndFrameworksCache(CosmosDbQueryDispatcher.Object);

            var submissionId = await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            await WithSqlQueryDispatcher(async dispatcher =>
            {
                var initializer = new FlowModelInitializer(CosmosDbQueryDispatcher.Object, dispatcher, standardsAndFrameworksCache);

                // Act
                var model = await initializer.Initialize(providerId);

                // Assert
                Assert.True(model.GotApprenticeshipDetails);
                Assert.Equal(contactEmail, model.ApprenticeshipContactEmail);
                Assert.Equal(contactTelephone, model.ApprenticeshipContactTelephone);
                Assert.Equal(contactWebsite, model.ApprenticeshipContactWebsite);
                Assert.Equal(apprenticeshipId, model.ApprenticeshipId);
                Assert.True(model.ApprenticeshipIsNational);
                Assert.Equal(ApprenticeshipLocationType.EmployerBased, model.ApprenticeshipLocationType);
                Assert.Equal(marketingInfo, model.ApprenticeshipMarketingInformation);
                Assert.Null(model.ApprenticeshipClassroomLocations);
                Assert.Equal(website, model.ApprenticeshipWebsite);
                Assert.True(model.ApprenticeshipStandardOrFramework.IsFramework);
                Assert.False(model.ApprenticeshipStandardOrFramework.IsStandard);
            });
        }

        [Fact]
        public async Task Initialize_NoSubmission_DoesNotPopulateApprenticeshipFields()
        {
            // Arrange
            var ukprn = 12345;
            var marketingInfo = "example marketing information";

            var providerId = await TestData.CreateProvider(
                ukprn: ukprn,
                providerName: "Provider 1",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Submitted,
                marketingInformation: marketingInfo);

            var standardsAndFrameworksCache = new StandardsAndFrameworksCache(CosmosDbQueryDispatcher.Object);

            await WithSqlQueryDispatcher(async dispatcher =>
            {
                var initializer = new FlowModelInitializer(CosmosDbQueryDispatcher.Object, dispatcher, standardsAndFrameworksCache);

                // Act
                var model = await initializer.Initialize(providerId);

                // Assert
                Assert.False(model.GotApprenticeshipDetails);
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
        public async Task Initialize_BothLocationType_InitializesModelCorrectly()
        {
            // Arrange
            var ukprn = 12346;
            var adminUserId = $"admin-user";
            var contactTelephone = "1111 111 1111";
            var website = "https://somerandomprovider.com/apprenticeship";
            var contactWebsite = "https://somerandomprovider.com";
            var marketingInfo = "Providing Online training";
            var regions = new List<string> { "123" };
            var contactEmail = "somecontact@nonexistentprovider.com";
            var radius = 10;
            var deliveryMode = ApprenticeshipDeliveryMode.BlockRelease;
            var providerId = await TestData.CreateProvider(
                ukprn: ukprn,
                providerName: "Provider 1",
                apprenticeshipQAStatus: ApprenticeshipQAStatus.Submitted);

            var providerUserId = $"{ukprn}-user";
            var user = await TestData.CreateUser(providerUserId, "somebody@provider1.com", "Provider 1", "Person", providerId);
            var adminUser = await TestData.CreateUser(adminUserId, "admin@provider.com", "admin", "admin", null);
            var framework = await TestData.CreateFramework(1, 1, 1, "Test Framework");
            var venueId = await TestData.CreateVenue(providerId);

            var venue = await CosmosDbQueryDispatcher.Object.ExecuteQuery(new GetVenueById() { VenueId = venueId });

            var apprenticeshipId = await TestData.CreateApprenticeship(providerId,
                framework,
                createdBy: user,
                contactEmail: contactEmail,
                contactTelephone: contactTelephone,
                contactWebsite: contactWebsite,
                marketingInformation: marketingInfo,
                website: website,
                locations: new[]
                {
                    CreateApprenticeshipLocation.CreateNational(),
                    CreateApprenticeshipLocation.CreateFromVenue(
                        venue,
                        radius,
                        new[] { deliveryMode })
                }); 

            var standardsAndFrameworksCache = new StandardsAndFrameworksCache(CosmosDbQueryDispatcher.Object);

            var submissionId = await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            await WithSqlQueryDispatcher(async dispatcher =>
            {
                var initializer = new FlowModelInitializer(CosmosDbQueryDispatcher.Object, dispatcher, standardsAndFrameworksCache);

                // Act
                var model = await initializer.Initialize(providerId);

                // Assert
                Assert.True(model.GotApprenticeshipDetails);
                Assert.Equal(contactEmail, model.ApprenticeshipContactEmail);
                Assert.Equal(contactTelephone, model.ApprenticeshipContactTelephone);
                Assert.Equal(contactWebsite, model.ApprenticeshipContactWebsite);
                Assert.Equal(apprenticeshipId, model.ApprenticeshipId);
                Assert.True(model.ApprenticeshipIsNational);
                Assert.Equal(ApprenticeshipLocationType.ClassroomBasedAndEmployerBased, model.ApprenticeshipLocationType);
                Assert.Equal(marketingInfo, model.ApprenticeshipMarketingInformation);
                Assert.Collection(
                    model.ApprenticeshipClassroomLocations.Values,
                    location =>
                    {
                        Assert.Equal(venueId, location.VenueId);
                        Assert.Equal(radius, location.Radius);
                        Assert.Contains(deliveryMode, location.DeliveryModes);
                    });
                Assert.Equal(website, model.ApprenticeshipWebsite);
                Assert.True(model.ApprenticeshipStandardOrFramework.IsFramework);
                Assert.False(model.ApprenticeshipStandardOrFramework.IsStandard);
            });
        }

        [Fact]
        public async Task Initialize_EmployerBasedLocationType_PopulatesModelCorrectly()
        {
            // Arrange
            var ukprn = 12346;
            var adminUserId = $"admin-user";
            var contactTelephone = "1111 111 1111";
            var website = "https://somerandomprovider.com/apprenticeship";
            var contactWebsite = "https://somerandomprovider.com";
            var marketingInfo = "Providing Online training";
            var contactEmail = "somecontact@nonexistentprovider.com";

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
                contactEmail: contactEmail,
                contactTelephone: contactTelephone,
                contactWebsite: contactWebsite,
                marketingInformation: marketingInfo,
                website: website,
                locations: new[]
                {
                    CreateApprenticeshipLocation.CreateNational()
                });

            var standardsAndFrameworksCache = new StandardsAndFrameworksCache(CosmosDbQueryDispatcher.Object);

            var submissionId = await TestData.CreateApprenticeshipQASubmission(
                providerId,
                submittedOn: Clock.UtcNow,
                submittedByUserId: providerUserId,
                providerMarketingInformation: "The overview",
                apprenticeshipIds: new[] { apprenticeshipId });

            await WithSqlQueryDispatcher(async dispatcher =>
            {
                var initializer = new FlowModelInitializer(CosmosDbQueryDispatcher.Object, dispatcher, standardsAndFrameworksCache);

                // Act
                var model = await initializer.Initialize(providerId);

                // Assert
                Assert.True(model.GotApprenticeshipDetails);
                Assert.Equal(contactEmail, model.ApprenticeshipContactEmail);
                Assert.Equal(contactTelephone, model.ApprenticeshipContactTelephone);
                Assert.Equal(contactWebsite, model.ApprenticeshipContactWebsite);
                Assert.Equal(apprenticeshipId, model.ApprenticeshipId);
                Assert.True(model.ApprenticeshipIsNational);
                Assert.Equal(ApprenticeshipLocationType.EmployerBased, model.ApprenticeshipLocationType);
                Assert.Equal(marketingInfo, model.ApprenticeshipMarketingInformation);
                Assert.Null(model.ApprenticeshipClassroomLocations);
                Assert.Equal(website, model.ApprenticeshipWebsite);
                Assert.True(model.ApprenticeshipStandardOrFramework.IsFramework);
                Assert.False(model.ApprenticeshipStandardOrFramework.IsStandard);
            });
        }
    }
}
