using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Common.Interfaces;
using Dfc.CourseDirectory.Core.BinaryStorageProvider;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Models.Interfaces.Apprenticeships;
using Dfc.CourseDirectory.Models.Models.Auth;
using Dfc.CourseDirectory.Models.Models.Venues;
using Dfc.CourseDirectory.Services.BulkUploadService;
using Dfc.CourseDirectory.Services.Interfaces;
using Dfc.CourseDirectory.Services.Interfaces.ApprenticeshipService;
using Dfc.CourseDirectory.Services.Interfaces.BlobStorageService;
using Dfc.CourseDirectory.Services.Interfaces.BulkUploadService;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Dfc.CourseDirectory.Services.Interfaces.ProviderService;
using Dfc.CourseDirectory.Services.Interfaces.VenueService;
using Dfc.CourseDirectory.Services.VenueService;
using Dfc.CourseDirectory.Testing;
using Dfc.CourseDirectory.Web.Controllers;
using Dfc.CourseDirectory.Web.Helpers;
using Dfc.CourseDirectory.WebV2;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using VenueStatus = Dfc.CourseDirectory.Models.Models.Venues.VenueStatus;

namespace Dfc.CourseDirectory.Web.Tests.Controllers
{
    public class BulkUploadApprenticeshipsControllerIntegrationTests
    {
        private Mock<IVenueService> _venueService;
        private Mock<IStandardsAndFrameworksCache> _standardsAndFrameworksCache;
        private Mock<IApprenticeshipService> _apprenticeshipService;
        private Mock<IBlobStorageService> _blobStorageService;
        private Mock<ICourseService> _courseService;
        private Mock<IProviderService> _providerService;
        private Mock<IBinaryStorageProvider> _binaryStorageProvider;
        private ISession _session;
        private ClaimsPrincipal _claimsPrincipal;
        private HttpContext _httpContext;
        private Mock<IUserHelper> _userHelper;
        private IOptions<ApprenticeshipBulkUploadSettings> _apprenticeshipBulkUploadSettings;

        private IApprenticeshipBulkUploadService _apprenticeshipBulkUploadService;
        private BulkUploadApprenticeshipsController _controller;

        public BulkUploadApprenticeshipsControllerIntegrationTests()
        {
            _venueService = new Mock<IVenueService>();
            _standardsAndFrameworksCache = new Mock<IStandardsAndFrameworksCache>();
            _apprenticeshipService = new Mock<IApprenticeshipService>();
            _blobStorageService = new Mock<IBlobStorageService>();
            _courseService = new Mock<ICourseService>();
            _providerService = new Mock<IProviderService>();
            _binaryStorageProvider = new Mock<IBinaryStorageProvider>();
            _session = new FakeSession();
            _claimsPrincipal = new ClaimsPrincipal();
            _httpContext = new DefaultHttpContext()
            {
                Session = _session,
                User = _claimsPrincipal
            };
            _userHelper = new Mock<IUserHelper>();
            _apprenticeshipBulkUploadSettings = Options.Create(new ApprenticeshipBulkUploadSettings()
            {
                ProcessSynchronouslyRowLimit = 100
            });

            // Need to be able to resolve the service via IApprenticeshipBulkUploadService
            var serviceProvider = new ServiceCollection()
                .AddTransient<IApprenticeshipBulkUploadService>(_ => _apprenticeshipBulkUploadService)
                .BuildServiceProvider();

            _apprenticeshipBulkUploadService = new ApprenticeshipBulkUploadService(
                NullLogger<ApprenticeshipBulkUploadService>.Instance,
                _apprenticeshipService.Object,
                _venueService.Object,
                _standardsAndFrameworksCache.Object,
                _binaryStorageProvider.Object,
                new ExecuteImmediatelyBackgroundWorkScheduler(serviceProvider.GetRequiredService<IServiceScopeFactory>()),
                _apprenticeshipBulkUploadSettings);

            _controller = new BulkUploadApprenticeshipsController(
                NullLogger<BulkUploadApprenticeshipsController>.Instance,
                _apprenticeshipBulkUploadService,
                _apprenticeshipService.Object,
                _blobStorageService.Object,
                _courseService.Object,
                _providerService.Object,
                _userHelper.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = _httpContext
                }
            };
        }

        [Fact]
        public async Task Index_WithBulkUploadFile_UploadsApprenticeshipsAndReturnsRedirectToActionResult()
        {
            var ukPrn = 12345678;
            var authUserDetails = new AuthUserDetails
            {
                UKPRN = ukPrn.ToString(),
                ProviderID = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Email = "test@test.com"
            };

            _session.SetInt32("UKPRN", ukPrn);

            _blobStorageService.SetupGet(s => s.InlineProcessingThreshold).Returns(400);

            _userHelper.Setup(s => s.GetUserDetailsFromClaims(It.IsAny<IEnumerable<Claim>>(), It.IsAny<int?>()))
                .Returns(authUserDetails);

            _standardsAndFrameworksCache.Setup(s => s.GetStandard(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync<int, int, IStandardsAndFrameworksCache, Standard>((c, v) => new Standard { StandardCode = c, Version = v });

            _standardsAndFrameworksCache.Setup(s => s.GetFramework(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync<int, int, int, IStandardsAndFrameworksCache, Framework>((c, t, p) => new Framework { FrameworkCode = c, ProgType = t, PathwayCode = p });

            _venueService.Setup(s => s.SearchAsync(It.IsAny<IVenueSearchCriteria>()))
                .ReturnsAsync<IVenueSearchCriteria, IVenueService, IResult<IVenueSearchResult>>(c => Result.Ok<IVenueSearchResult>(new VenueSearchResult(new[] { new Venue { VenueName = "Fenestra Centre Scunthorpe", Status = VenueStatus.Live } })));

            _apprenticeshipService.Setup(s => s.ChangeApprenticeshipStatusesForUKPRNSelection(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(Result.Ok());

            var addedApprenticeships = new List<IApprenticeship>();
            _apprenticeshipService.Setup(s => s.AddApprenticeships(It.IsAny<IEnumerable<IApprenticeship>>()))
                .Callback<IEnumerable<IApprenticeship>>(a => addedApprenticeships.AddRange(a))
                .ReturnsAsync(Result.Ok());

            const string csv = "STANDARD_CODE,STANDARD_VERSION,FRAMEWORK_CODE,FRAMEWORK_PROG_TYPE,FRAMEWORK_PATHWAY_CODE,APPRENTICESHIP_INFORMATION,APPRENTICESHIP_WEBPAGE,CONTACT_EMAIL,CONTACT_PHONE,CONTACT_URL,DELIVERY_METHOD,VENUE,RADIUS,DELIVERY_MODE,ACROSS_ENGLAND, NATIONAL_DELIVERY,REGION,SUB_REGION\r\n" +
                "105,1,,,,\"This apprenticeship is applicable to any industry and perfect for those already in a team leading role or entering a management role for the first time.It lasts around 15 months and incorporates 14 one day modules plus on - the job learning and mentoring.It involves managing projects, leading and managing teams, change, financial management and coaching.If you choose to do this qualification with Azesta, you can expect exciting and engaging day long modules fully utilising experiential learning methods(one per month), high quality tutorial support, access to e-mail and telephone support whenever you need it and great results in your end point assessments.\",http://www.azesta.co.uk/apprenticeships/,info@azesta.co.uk,1423711904,http://www.azesta.co.uk/contact-us/,Both,Fenestra Centre Scunthorpe,100,Employer,No,,,\r\n" +
                "104,1,,,,\"This apprenticeship is great for current managers. It involves managing projects, leading and managing teams, change, financial and resource management, talent management and coaching and mentoring. It takes around 2.5 years to complete and if you choose to do this qualification with Azesta, you can expect exciting and engaging day long modules fully utilising experiential learning methods (one per month), high quality tutorial support, access to e-mail and telephone support whenever you need it and great results in your end point assessments.\",http://www.azesta.co.uk/apprenticeships/,info@azesta.co.uk,1423711904,http://www.azesta.co.uk,Both,Fenestra Centre Scunthorpe,100,Employer,Yes,,,\r\n";

            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));
            var bulkUploadFile = new FormFile(stream, 0, stream.Length, "bulkUploadFile", "Test.csv")
            {
                Headers = new HeaderDictionary
                {
                    { "Content-Disposition", "filename" }
                }
            };

            var result = await _controller.Index(bulkUploadFile);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("PublishYourFile", redirectResult.ActionName);
            Assert.Equal("BulkUploadApprenticeships", redirectResult.ControllerName);

            Assert.True(addedApprenticeships.Any());
            Assert.Equal(2, addedApprenticeships.Count);

            var apprenticeship1 = addedApprenticeships.First();
            var apprenticeship2 = addedApprenticeships.Skip(1).First();

            Assert.Equal(ukPrn, apprenticeship1.ProviderUKPRN);
            Assert.Equal(authUserDetails.ProviderID, apprenticeship1.ProviderId);
            Assert.Equal(authUserDetails.UserId.ToString(), apprenticeship1.CreatedBy);
            Assert.Equal(105, apprenticeship1.StandardCode);
            Assert.Equal("This apprenticeship is applicable to any industry and perfect for those already in a team leading role or entering a management role for the first time.It lasts around 15 months and incorporates 14 one day modules plus on - the job learning and mentoring.It involves managing projects, leading and managing teams, change, financial management and coaching.If you choose to do this qualification with Azesta, you can expect exciting and engaging day long modules fully utilising experiential learning methods(one per month), high quality tutorial support, access to e-mail and telephone support whenever you need it and great results in your end point assessments.", apprenticeship1.MarketingInformation);
            Assert.Equal("http://www.azesta.co.uk/apprenticeships/", apprenticeship1.Url);
            Assert.Equal("info@azesta.co.uk", apprenticeship1.ContactEmail);
            Assert.Equal("1423711904", apprenticeship1.ContactTelephone);
            Assert.Equal("http://www.azesta.co.uk/contact-us/", apprenticeship1.ContactWebsite);

            var apprenticeship1Location = apprenticeship1.ApprenticeshipLocations.Single();
            Assert.Equal("Fenestra Centre Scunthorpe", apprenticeship1Location.Name);
            Assert.Equal(authUserDetails.Email, apprenticeship1Location.CreatedBy);

            Assert.Equal(ukPrn, apprenticeship2.ProviderUKPRN);
            Assert.Equal(authUserDetails.ProviderID, apprenticeship2.ProviderId);
            Assert.Equal(authUserDetails.UserId.ToString(), apprenticeship2.CreatedBy);
            Assert.Equal(104, apprenticeship2.StandardCode);
            Assert.Equal("This apprenticeship is great for current managers. It involves managing projects, leading and managing teams, change, financial and resource management, talent management and coaching and mentoring. It takes around 2.5 years to complete and if you choose to do this qualification with Azesta, you can expect exciting and engaging day long modules fully utilising experiential learning methods (one per month), high quality tutorial support, access to e-mail and telephone support whenever you need it and great results in your end point assessments.", apprenticeship2.MarketingInformation);
            Assert.Equal("http://www.azesta.co.uk/apprenticeships/", apprenticeship2.Url);
            Assert.Equal("info@azesta.co.uk", apprenticeship2.ContactEmail);
            Assert.Equal("1423711904", apprenticeship2.ContactTelephone);
            Assert.Equal("http://www.azesta.co.uk", apprenticeship2.ContactWebsite);

            var apprenticeship2Location = apprenticeship2.ApprenticeshipLocations.Single();
            Assert.Equal("Fenestra Centre Scunthorpe", apprenticeship2Location.Name);
            Assert.Equal(authUserDetails.Email, apprenticeship2Location.CreatedBy);
        }
    }
}