using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Services;
using Moq;
using Xunit;

namespace Dfc.CourseDirectory.Core.Tests.Services
{
    public class CourseTypeServiceTests
    {
        private readonly Mock<ISqlQueryDispatcher> _mockSqlQueryDispatcher;

        public CourseTypeServiceTests()
        {
            _mockSqlQueryDispatcher = new Mock<ISqlQueryDispatcher>();
        }

        [Fact]
        public async Task GetCourseType_WhenLARSCategoryRefIs3AndLearnAimRefTitleStartsWithTLevel_ReturnsCourseTypeAsTLevels()
        {
            // Arrange
            var courseTypeService = new CourseTypeService(_mockSqlQueryDispatcher.Object);
            var learnAimRef = "00214511";
            var expectedCourseType = CourseType.TLevels;
            var providerId = new Guid();
            ArrangeObjects(learnAimRef);

            // Act
            var courseType = await courseTypeService.GetCourseType(learnAimRef, providerId);

            // Assert            
            Assert.Equal(expectedCourseType, courseType);

            _mockSqlQueryDispatcher.Verify(p => p.ExecuteQuery(It.IsAny<GetLarsCourseType>()), Times.Once);
        }

        [Fact]
        public async Task GetCourseType_WhenLARSCategoryRefIs3AndLearnAimRefTitleDoesNotStartWithTLevel_ReturnsCourseTypeAsNull()
        {
            // Arrange
            var courseTypeService = new CourseTypeService(_mockSqlQueryDispatcher.Object);
            var learnAimRef = "00214512";
            CourseType? expectedCourseType = null;
            var providerId = new Guid();
            ArrangeObjects(learnAimRef);

            // Act
            var courseType = await courseTypeService.GetCourseType(learnAimRef, providerId);

            // Assert
            Assert.Equal(expectedCourseType, courseType);
            _mockSqlQueryDispatcher.Verify(p => p.ExecuteQuery(It.IsAny<GetLarsCourseType>()), Times.Once);
        }

        [Fact]
        public async Task GetCourseType_WhenLARSCategoryRefIs24_ReturnsCourseTypeAsEssentialSkills()
        {
            // Arrange
            var courseTypeService = new CourseTypeService(_mockSqlQueryDispatcher.Object);
            var learnAimRef = "0021452";
            var expectedCourseType = CourseType.EssentialSkills;
            var providerId = new Guid();
            ArrangeObjects(learnAimRef);

            // Act
            var courseType = await courseTypeService.GetCourseType(learnAimRef, providerId);

            // Assert
            Assert.Equal(expectedCourseType, courseType);
            _mockSqlQueryDispatcher.Verify(p => p.ExecuteQuery(It.IsAny<GetLarsCourseType>()), Times.Once);
        }

        [Fact]
        public async Task GetCourseType_WhenLARSCategoryRefIs29_ReturnsCourseTypeAsEssentialSkills()
        {
            // Arrange
            var courseTypeService = new CourseTypeService(_mockSqlQueryDispatcher.Object);
            var learnAimRef = "0021453";
            var expectedCourseType = CourseType.EssentialSkills;
            var providerId = new Guid();
            ArrangeObjects(learnAimRef);

            // Act
            var courseType = await courseTypeService.GetCourseType(learnAimRef, providerId);

            // Assert
            Assert.Equal(expectedCourseType, courseType);
            _mockSqlQueryDispatcher.Verify(p => p.ExecuteQuery(It.IsAny<GetLarsCourseType>()), Times.Once);
        }

        [Fact]
        public async Task GetCourseType_WhenLARSCategoryRefIs37AndLearnAimRefTitleContainsGCSEEnglishLanguageText_ReturnsCourseTypeAsEssentialSkills()
        {
            // Arrange
            var courseTypeService = new CourseTypeService(_mockSqlQueryDispatcher.Object);
            var learnAimRef = "00214541";
            var expectedCourseType = CourseType.EssentialSkills;
            var providerId = new Guid();
            ArrangeObjects(learnAimRef);

            // Act
            var courseType = await courseTypeService.GetCourseType(learnAimRef, providerId);

            // Assert
            Assert.Equal(expectedCourseType, courseType);
            _mockSqlQueryDispatcher.Verify(p => p.ExecuteQuery(It.IsAny<GetLarsCourseType>()), Times.Once);
        }

        [Fact]
        public async Task GetCourseType_WhenLARSCategoryRefIs37AndLearnAimRefTitleContainsGCSEEnglishLiteratureText_ReturnsCourseTypeAsEssentialSkills()
        {
            // Arrange
            var courseTypeService = new CourseTypeService(_mockSqlQueryDispatcher.Object);
            var learnAimRef = "00214542";
            var expectedCourseType = CourseType.EssentialSkills;
            var providerId = new Guid();
            ArrangeObjects(learnAimRef);

            // Act
            var courseType = await courseTypeService.GetCourseType(learnAimRef, providerId);

            // Assert
            Assert.Equal(expectedCourseType, courseType);
            _mockSqlQueryDispatcher.Verify(p => p.ExecuteQuery(It.IsAny<GetLarsCourseType>()), Times.Once);
        }

        [Fact]
        public async Task GetCourseType_WhenLARSCategoryRefIs37AndLearnAimRefTitleDoesNotContainGCSEText_ReturnsCourseTypeAsNull()
        {
            // Arrange
            var courseTypeService = new CourseTypeService(_mockSqlQueryDispatcher.Object);
            var learnAimRef = "00214543";
            CourseType? expectedCourseType = null;
            var providerId = new Guid();
            ArrangeObjects(learnAimRef);

            // Act
            var courseType = await courseTypeService.GetCourseType(learnAimRef, providerId);

            // Assert
            Assert.Equal(expectedCourseType, courseType);
            _mockSqlQueryDispatcher.Verify(p => p.ExecuteQuery(It.IsAny<GetLarsCourseType>()), Times.Once);
        }

        [Fact]
        public async Task GetCourseType_WhenLARSCategoryRefIs39_ReturnsCourseTypeAsEssentialSkills()
        {
            // Arrange
            var courseTypeService = new CourseTypeService(_mockSqlQueryDispatcher.Object);
            var learnAimRef = "0021455";
            var expectedCourseType = CourseType.EssentialSkills;
            var providerId = new Guid();
            ArrangeObjects(learnAimRef);

            // Act
            var courseType = await courseTypeService.GetCourseType(learnAimRef, providerId);

            // Assert
            Assert.Equal(expectedCourseType, courseType);
            _mockSqlQueryDispatcher.Verify(p => p.ExecuteQuery(It.IsAny<GetLarsCourseType>()), Times.Once);
        }

        [Fact]
        public async Task GetCourseType_WhenLARSCategoryRefIs40AndLearnAimRefTitleContainsESOL_ReturnsCourseTypeAsEssentialSkills()
        {
            // Arrange
            var courseTypeService = new CourseTypeService(_mockSqlQueryDispatcher.Object);
            var learnAimRef = "00214561";
            var expectedCourseType = CourseType.EssentialSkills;
            var providerId = new Guid();
            ArrangeObjects(learnAimRef);

            // Act
            var courseType = await courseTypeService.GetCourseType(learnAimRef, providerId);

            // Assert
            Assert.Equal(expectedCourseType, courseType);
            _mockSqlQueryDispatcher.Verify(p => p.ExecuteQuery(It.IsAny<GetLarsCourseType>()), Times.Once);
        }

        [Fact]
        public async Task GetCourseType_WhenLARSCategoryRefIs40AndLearnAimRefTitleContainsGCSEEnglishLanguageText_ReturnsCourseTypeAsEssentialSkills()
        {
            // Arrange
            var courseTypeService = new CourseTypeService(_mockSqlQueryDispatcher.Object);
            var learnAimRef = "00214562";
            var expectedCourseType = CourseType.EssentialSkills;
            var providerId = new Guid();
            ArrangeObjects(learnAimRef);

            // Act
            var courseType = await courseTypeService.GetCourseType(learnAimRef, providerId);

            // Assert
            Assert.Equal(expectedCourseType, courseType);
            _mockSqlQueryDispatcher.Verify(p => p.ExecuteQuery(It.IsAny<GetLarsCourseType>()), Times.Once);
        }

        [Fact]
        public async Task GetCourseType_WhenLARSCategoryRefIs40AndLearnAimRefTitleContainsGCSEEnglishLiteratureText_ReturnsCourseTypeAsEssentialSkills()
        {
            // Arrange
            var courseTypeService = new CourseTypeService(_mockSqlQueryDispatcher.Object);
            var learnAimRef = "00214563";
            var expectedCourseType = CourseType.EssentialSkills;
            var providerId = new Guid();
            ArrangeObjects(learnAimRef);

            // Act
            var courseType = await courseTypeService.GetCourseType(learnAimRef, providerId);

            // Assert
            Assert.Equal(expectedCourseType, courseType);
            _mockSqlQueryDispatcher.Verify(p => p.ExecuteQuery(It.IsAny<GetLarsCourseType>()), Times.Once);
        }

        [Fact]
        public async Task GetCourseType_WhenLARSCategoryRefIs40AndLearnAimRefTitleDoesNotContainESOL_ReturnsCourseTypeAsNull()
        {
            // Arrange
            var courseTypeService = new CourseTypeService(_mockSqlQueryDispatcher.Object);
            var learnAimRef = "00214564";
            CourseType? expectedCourseType = null;
            var providerId = new Guid();
            ArrangeObjects(learnAimRef);

            // Act
            var courseType = await courseTypeService.GetCourseType(learnAimRef, providerId);

            // Assert
            Assert.Equal(expectedCourseType, courseType);
            _mockSqlQueryDispatcher.Verify(p => p.ExecuteQuery(It.IsAny<GetLarsCourseType>()), Times.Once);
        }

        [Fact]
        public async Task GetCourseType_WhenLARSCategoryRefIs42_ReturnsCourseTypeAsEssentialSkills()
        {
            // Arrange
            var courseTypeService = new CourseTypeService(_mockSqlQueryDispatcher.Object);
            var learnAimRef = "0021457";
            var expectedCourseType = CourseType.EssentialSkills;
            var providerId = new Guid();
            ArrangeObjects(learnAimRef);

            // Act
            var courseType = await courseTypeService.GetCourseType(learnAimRef, providerId);

            // Assert
            Assert.Equal(expectedCourseType, courseType);
            _mockSqlQueryDispatcher.Verify(p => p.ExecuteQuery(It.IsAny<GetLarsCourseType>()), Times.Once);
        }

        [Fact]
        public async Task GetCourseType_WhenLARSCategoryRefIs45AndProviderIsInEligibleList_ReturnsCourseTypeAsFreeCoursesForJobs()
        {
            // Arrange
            var courseTypeService = new CourseTypeService(_mockSqlQueryDispatcher.Object);
            var learnAimRef = "0021458";
            var expectedCourseType = CourseType.FreeCoursesForJobs;
            var providerId = new Guid();
            ArrangeObjects(learnAimRef);

            var eligibleProvidersList = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), providerId };

            _mockSqlQueryDispatcher.Setup(m => m.ExecuteQuery(It.IsAny<GetFcfjEligibleProvidersList>())).ReturnsAsync(eligibleProvidersList);

            // Act
            var courseType = await courseTypeService.GetCourseType(learnAimRef, providerId);

            // Assert
            Assert.Equal(expectedCourseType, courseType);
            _mockSqlQueryDispatcher.Verify(p => p.ExecuteQuery(It.IsAny<GetLarsCourseType>()), Times.Once);
        }

        [Fact]
        public async Task GetCourseType_WhenLARSCategoryRefIs45AndProviderIsNotInEligibleList_ReturnsCourseTypeAsNull()
        {
            // Arrange
            var courseTypeService = new CourseTypeService(_mockSqlQueryDispatcher.Object);
            var learnAimRef = "0021458";
            CourseType? expectedCourseType = null;
            var providerId = new Guid();
            ArrangeObjects(learnAimRef);

            var eligibleProvidersList = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

            _mockSqlQueryDispatcher.Setup(m => m.ExecuteQuery(It.IsAny<GetFcfjEligibleProvidersList>())).ReturnsAsync(eligibleProvidersList);

            // Act
            var courseType = await courseTypeService.GetCourseType(learnAimRef, providerId);

            // Assert
            Assert.Equal(expectedCourseType, courseType);
            _mockSqlQueryDispatcher.Verify(p => p.ExecuteQuery(It.IsAny<GetLarsCourseType>()), Times.Once);
            _mockSqlQueryDispatcher.Verify(p => p.ExecuteQuery(It.IsAny<GetFcfjEligibleProvidersList>()), Times.Once);
        }

        [Fact]
        public async Task GetCourseType_WhenLARSCategoryRefIs46AndProviderIsInEligibleList_ReturnsCourseTypeAsFreeCoursesForJobs()
        {
            // Arrange
            var courseTypeService = new CourseTypeService(_mockSqlQueryDispatcher.Object);
            var learnAimRef = "0021459";
            var expectedCourseType = CourseType.FreeCoursesForJobs;
            var providerId = new Guid();
            ArrangeObjects(learnAimRef);

            var eligibleProvidersList = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), providerId };
            _mockSqlQueryDispatcher.Setup(m => m.ExecuteQuery(It.IsAny<GetFcfjEligibleProvidersList>())).ReturnsAsync(eligibleProvidersList);

            // Act
            var courseType = await courseTypeService.GetCourseType(learnAimRef, providerId);

            // Assert
            Assert.Equal(expectedCourseType, courseType);
            _mockSqlQueryDispatcher.Verify(p => p.ExecuteQuery(It.IsAny<GetLarsCourseType>()), Times.Once);
            _mockSqlQueryDispatcher.Verify(p => p.ExecuteQuery(It.IsAny<GetFcfjEligibleProvidersList>()), Times.Once);
        }

        [Fact]
        public async Task GetCourseType_WhenLARSCategoryRefIs46AndProviderIsNotInEligibleList_ReturnsCourseTypeAsNull()
        {
            // Arrange
            var courseTypeService = new CourseTypeService(_mockSqlQueryDispatcher.Object);
            var learnAimRef = "0021459";
            CourseType? expectedCourseType = null;
            var providerId = new Guid();
            ArrangeObjects(learnAimRef);

            var eligibleProvidersList = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            _mockSqlQueryDispatcher.Setup(m => m.ExecuteQuery(It.IsAny<GetFcfjEligibleProvidersList>())).ReturnsAsync(eligibleProvidersList);


            // Act
            var courseType = await courseTypeService.GetCourseType(learnAimRef, providerId);

            // Assert
            Assert.Equal(expectedCourseType, courseType);
            _mockSqlQueryDispatcher.Verify(p => p.ExecuteQuery(It.IsAny<GetLarsCourseType>()), Times.Once);
            _mockSqlQueryDispatcher.Verify(p => p.ExecuteQuery(It.IsAny<GetFcfjEligibleProvidersList>()), Times.Once);
        }

        [Fact]
        public async Task GetCourseType_WhenLARSCategoryRefIs48AndProviderIsInEligibleList_ReturnsCourseTypeAsFreeCoursesForJobs()
        {
            // Arrange
            var courseTypeService = new CourseTypeService(_mockSqlQueryDispatcher.Object);
            var learnAimRef = "0021460";
            var expectedCourseType = CourseType.FreeCoursesForJobs;
            var providerId = new Guid();
            ArrangeObjects(learnAimRef);

            var eligibleProvidersList = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), providerId };
            _mockSqlQueryDispatcher.Setup(m => m.ExecuteQuery(It.IsAny<GetFcfjEligibleProvidersList>())).ReturnsAsync(eligibleProvidersList);

            // Act
            var courseType = await courseTypeService.GetCourseType(learnAimRef, providerId);

            // Assert
            Assert.Equal(expectedCourseType, courseType);
            _mockSqlQueryDispatcher.Verify(p => p.ExecuteQuery(It.IsAny<GetLarsCourseType>()), Times.Once);
            _mockSqlQueryDispatcher.Verify(p => p.ExecuteQuery(It.IsAny<GetFcfjEligibleProvidersList>()), Times.Once);
        }

        [Fact]
        public async Task GetCourseType_WhenLARSCategoryRefIs49_ReturnsCourseTypeAsFreeCoursesForJobs()
        {
            // Arrange
            var courseTypeService = new CourseTypeService(_mockSqlQueryDispatcher.Object);
            var learnAimRef = "0021461";
            var expectedCourseType = CourseType.FreeCoursesForJobs;
            var providerId = new Guid();
            ArrangeObjects(learnAimRef);

            var eligibleProvidersList = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), providerId };
            _mockSqlQueryDispatcher.Setup(m => m.ExecuteQuery(It.IsAny<GetFcfjEligibleProvidersList>())).ReturnsAsync(eligibleProvidersList);

            // Act
            var courseType = await courseTypeService.GetCourseType(learnAimRef, providerId);

            // Assert
            Assert.Equal(expectedCourseType, courseType);
            _mockSqlQueryDispatcher.Verify(p => p.ExecuteQuery(It.IsAny<GetLarsCourseType>()), Times.Once);
        }

        [Fact]
        public async Task GetCourseType_WhenLARSCategoryRefIs56AndProviderIsInEligibleList_ReturnsCourseTypeAsFreeCoursesForJobs()
        {
            // Arrange
            var courseTypeService = new CourseTypeService(_mockSqlQueryDispatcher.Object);
            var learnAimRef = "0021464";
            var expectedCourseType = CourseType.FreeCoursesForJobs;
            var providerId = new Guid();
            ArrangeObjects(learnAimRef);

            var eligibleProvidersList = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), providerId };
            _mockSqlQueryDispatcher.Setup(m => m.ExecuteQuery(It.IsAny<GetFcfjEligibleProvidersList>())).ReturnsAsync(eligibleProvidersList);

            // Act
            var courseType = await courseTypeService.GetCourseType(learnAimRef, providerId);

            // Assert
            Assert.Equal(expectedCourseType, courseType);
            _mockSqlQueryDispatcher.Verify(p => p.ExecuteQuery(It.IsAny<GetLarsCourseType>()), Times.Once);
        }

        [Fact]
        public async Task GetCourseType_WhenLARSCategoryRefIs55_ReturnsCourseTypeAsHTQs()
        {
            // Arrange
            var courseTypeService = new CourseTypeService(_mockSqlQueryDispatcher.Object);
            var learnAimRef = "0021462";
            var expectedCourseType = CourseType.HTQs;
            var providerId = new Guid();
            ArrangeObjects(learnAimRef);

            // Act
            var courseType = await courseTypeService.GetCourseType(learnAimRef, providerId);

            // Assert
            Assert.Equal(expectedCourseType, courseType);
            _mockSqlQueryDispatcher.Verify(p => p.ExecuteQuery(It.IsAny<GetLarsCourseType>()), Times.Once);
        }

        [Fact]
        public async Task GetCourseType_WhenLARSCategoryRefIs63_ReturnsCourseTypeAsMultiply()
        {
            // Arrange
            var courseTypeService = new CourseTypeService(_mockSqlQueryDispatcher.Object);
            var learnAimRef = "0021463";
            var expectedCourseType = CourseType.Multiply;
            var providerId = new Guid();
            ArrangeObjects(learnAimRef);

            // Act
            var courseType = await courseTypeService.GetCourseType(learnAimRef, providerId);

            // Assert
            Assert.Equal(expectedCourseType, courseType);
            _mockSqlQueryDispatcher.Verify(p => p.ExecuteQuery(It.IsAny<GetLarsCourseType>()), Times.Once);
        }

        [Fact]
        public async Task GetCourseType_WhenLARSCategoryRefsAre3And24_ReturnsCourseTypeAsEssentialSkills()
        {
            // Arrange
            var courseTypeService = new CourseTypeService(_mockSqlQueryDispatcher.Object);
            var learnAimRef = "0021221";
            var expectedCourseType = CourseType.EssentialSkills;
            var providerId = new Guid();
            ArrangeObjects(learnAimRef);

            // Act
            var courseType = await courseTypeService.GetCourseType(learnAimRef, providerId);

            // Assert
            Assert.Equal(expectedCourseType, courseType);
            _mockSqlQueryDispatcher.Verify(p => p.ExecuteQuery(It.IsAny<GetLarsCourseType>()), Times.Once);
        }
        private void ArrangeObjects(string learnAimRef)
        {
            var larsCourseTypesList = new List<LarsCourseType>()
            {
                { new LarsCourseType { LearnAimRef = "00214511", CategoryRef = "3", CourseType = CourseType.TLevels, LearnAimRefTitle = "T Level Title" }},
                { new LarsCourseType { LearnAimRef = "00214512", CategoryRef = "3", CourseType = CourseType.TLevels, LearnAimRefTitle = "Title" }},
                { new LarsCourseType { LearnAimRef = "0021452", CategoryRef = "24", CourseType = CourseType.EssentialSkills, LearnAimRefTitle = "Title" }},
                { new LarsCourseType { LearnAimRef = "0021453", CategoryRef = "29", CourseType = CourseType.EssentialSkills, LearnAimRefTitle = "Title" }},
                { new LarsCourseType { LearnAimRef = "00214541", CategoryRef = "37", CourseType = CourseType.EssentialSkills, LearnAimRefTitle = "Title GCSE (9-1) in English Language Title" }},
                { new LarsCourseType { LearnAimRef = "00214542", CategoryRef = "37", CourseType = CourseType.EssentialSkills, LearnAimRefTitle = "Title GCSE (9-1) in English Literature Title" }},
                { new LarsCourseType { LearnAimRef = "00214543", CategoryRef = "37", CourseType = CourseType.EssentialSkills, LearnAimRefTitle = "Title" }},
                { new LarsCourseType { LearnAimRef = "0021455", CategoryRef = "39", CourseType = CourseType.EssentialSkills, LearnAimRefTitle = "Title" }},
                { new LarsCourseType { LearnAimRef = "00214561", CategoryRef = "40", CourseType = CourseType.EssentialSkills, LearnAimRefTitle = "Title ESOL Title" }},
                { new LarsCourseType { LearnAimRef = "00214562", CategoryRef = "40", CourseType = CourseType.EssentialSkills, LearnAimRefTitle = "Title GCSE (9-1) in English Language Title" }},
                { new LarsCourseType { LearnAimRef = "00214563", CategoryRef = "40", CourseType = CourseType.EssentialSkills, LearnAimRefTitle = "Title GCSE (9-1) in English Literature Title" }},
                { new LarsCourseType { LearnAimRef = "00214564", CategoryRef = "40", CourseType = CourseType.EssentialSkills, LearnAimRefTitle = "Title" }},
                { new LarsCourseType { LearnAimRef = "0021457", CategoryRef = "42", CourseType = CourseType.EssentialSkills, LearnAimRefTitle = "Title" }},
                { new LarsCourseType { LearnAimRef = "0021458", CategoryRef = "45", CourseType = CourseType.FreeCoursesForJobs, LearnAimRefTitle = "Title" }},
                { new LarsCourseType { LearnAimRef = "0021459", CategoryRef = "46", CourseType = CourseType.FreeCoursesForJobs, LearnAimRefTitle = "Title" }},
                { new LarsCourseType { LearnAimRef = "0021460", CategoryRef = "48", CourseType = CourseType.FreeCoursesForJobs, LearnAimRefTitle = "Title" }},
                { new LarsCourseType { LearnAimRef = "0021461", CategoryRef = "49", CourseType = CourseType.FreeCoursesForJobs, LearnAimRefTitle = "Title" }},
                { new LarsCourseType { LearnAimRef = "0021462", CategoryRef = "55", CourseType = CourseType.HTQs, LearnAimRefTitle = "Title" }},
                { new LarsCourseType { LearnAimRef = "0021464", CategoryRef = "56", CourseType = CourseType.FreeCoursesForJobs, LearnAimRefTitle = "Title" }},
                { new LarsCourseType { LearnAimRef = "0021463", CategoryRef = "63", CourseType = CourseType.Multiply, LearnAimRefTitle = "Title" }},
                { new LarsCourseType { LearnAimRef = "0021221", CategoryRef = "3", CourseType = CourseType.TLevels, LearnAimRefTitle = "Title Title Title" }},
                { new LarsCourseType { LearnAimRef = "0021221", CategoryRef = "24", CourseType = CourseType.EssentialSkills, LearnAimRefTitle = "Title" }},
                { new LarsCourseType { LearnAimRef = "0011123", CategoryRef = "23", CourseType = CourseType.EssentialSkills, LearnAimRefTitle = "Title" }},
                { new LarsCourseType { LearnAimRef = "0011124", CategoryRef = "27", CourseType = CourseType.EssentialSkills, LearnAimRefTitle = "Title" }},
                { new LarsCourseType { LearnAimRef = "0011125", CategoryRef = "28", CourseType = CourseType.EssentialSkills, LearnAimRefTitle = "Title" }},
                { new LarsCourseType { LearnAimRef = "0011126", CategoryRef = "75", CourseType = CourseType.EssentialSkills, LearnAimRefTitle = "Title" }}


            };
            var learningDelivery = new List<LearningDelivery>() { new LearningDelivery { LearnAimRef = "0011111", LearnAimRefTitle ="GCE A Level" },
            new LearningDelivery { LearnAimRef = "0011112", LearnAimRefTitle ="GCE A Level" },
            new LearningDelivery { LearnAimRef = "0011113", LearnAimRefTitle ="GCE A2 Level" },
            new LearningDelivery { LearnAimRef = "0011114", LearnAimRefTitle ="GCE AS Level" },
            new LearningDelivery { LearnAimRef = "0011115", LearnAimRefTitle ="RQF" },
            new LearningDelivery { LearnAimRef = "0011116", LearnAimRefTitle ="VRQ" },
            new LearningDelivery { LearnAimRef = "0011117", LearnAimRefTitle ="Degree" },
            new LearningDelivery { LearnAimRef = "0011118", LearnAimRefTitle ="BA (Hons)" },
            new LearningDelivery { LearnAimRef = "0011119", LearnAimRefTitle ="BSc (Hons)" },
            new LearningDelivery { LearnAimRef = "0011120", LearnAimRefTitle ="BEng (Hons)" },
            new LearningDelivery { LearnAimRef = "0011121", LearnAimRefTitle ="BSc (Ord/Hons)" },
            new LearningDelivery { LearnAimRef = "0011122", LearnAimRefTitle ="NVQ" },
            new LearningDelivery { LearnAimRef = "0011123", LearnAimRefTitle ="Non regulated" },
            new LearningDelivery { LearnAimRef = "0011124", LearnAimRefTitle ="Non regulated" },
            new LearningDelivery { LearnAimRef = "0011125", LearnAimRefTitle ="Non regulated" },
            new LearningDelivery { LearnAimRef = "0011126", LearnAimRefTitle ="Non regulated" },
            };

            var larsCourseTypesReadOnlyList = new ReadOnlyCollection<LarsCourseType>(larsCourseTypesList.Where(l => l.LearnAimRef == learnAimRef).ToList());
            var ldDictonary = new Dictionary<string, LearningDelivery>();
            if (learningDelivery.Any(x => x.LearnAimRef == learnAimRef))
            {
                ldDictonary.Add(learnAimRef, learningDelivery.First(x => x.LearnAimRef == learnAimRef));
            }
            var learningDeliveries = new ReadOnlyDictionary<string, LearningDelivery>(ldDictonary);
    
            _mockSqlQueryDispatcher.Setup(m => m.ExecuteQuery(It.IsAny<GetLarsCourseType>()))
                .ReturnsAsync(larsCourseTypesReadOnlyList);
            _mockSqlQueryDispatcher.Setup(m => m.ExecuteQuery(It.IsAny<GetLearningDeliveries>()))
                .ReturnsAsync(learningDeliveries);
        }
    }
}
