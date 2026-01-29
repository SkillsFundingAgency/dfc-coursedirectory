using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Bogus;
using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Extensions;
using Dfc.CourseDirectory.Core.Middleware;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Security;
using Dfc.CourseDirectory.Services.CourseService;
using Dfc.CourseDirectory.Services.Models.Regions;
using Dfc.CourseDirectory.WebV2.Controllers;
using Dfc.CourseDirectory.WebV2.Controllers.CopyCourse;
using Dfc.CourseDirectory.WebV2.ViewModels.CopyCourse;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.ControllerTests
{
    public class ProviderDataManagementControllerTests
    {
        private readonly Mock<IMediator> _mockMediator;

        public ProviderDataManagementControllerTests()
        {
            _mockMediator = new Mock<IMediator>();
        }

        [Fact]
        public void Index_GetRedirectToActiveProviders()
        {
            // Arrange
            var controller = new ProvidersDataManagementController(_mockMediator.Object);

            // Act
            var result = controller.Index(ViewModels.DataManagement.Providers.Home.ProviderUploadType.Active) as RedirectToActionResult;

            // Assert            
            Assert.NotNull(result);

            Assert.Equal("ActiveProviders", result.ActionName);
        }

        [Fact]
        public void Index_PostRedirectToActiveProviders()
        {
            // Arrange
            var controller = new ProvidersDataManagementController(_mockMediator.Object);

            // Act
            var result = controller.Index(new Dfc.CourseDirectory.WebV2.ViewModels.DataManagement.Providers.Home.ViewModel { 
                ProviderUploadType = ViewModels.DataManagement.Providers.Home.ProviderUploadType.Active}) as RedirectToActionResult;

            // Assert            
            Assert.NotNull(result);

            Assert.Equal("ActiveProviders", result.ActionName);
        }

        [Fact]
        public void Index_PosttRedirectToInActiveProviders()
        {
            // Arrange
            var controller = new ProvidersDataManagementController(_mockMediator.Object);

            // Act
            var result = controller.Index(new Dfc.CourseDirectory.WebV2.ViewModels.DataManagement.Providers.Home.ViewModel
            {
                ProviderUploadType = ViewModels.DataManagement.Providers.Home.ProviderUploadType.Inactive
            }) as RedirectToActionResult;

            // Assert            
            Assert.NotNull(result);

            Assert.Equal("InactiveProviders", result.ActionName);
        }
        
    }
}
