using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Validation;
using Dfc.CourseDirectory.Testing;
using FluentAssertions;
using Xunit;

namespace Dfc.CourseDirectory.WebV2.Tests.FeatureTests.DataManagement.Courses
{
    public class ResolveRowDescriptionTests : MvcTestBase
    {
        public ResolveRowDescriptionTests(CourseDirectoryApplicationFactory factory)
            : base(factory)
        {
        }

        [Theory]
        [InlineData(null)]
        [InlineData(UploadStatus.Created)]
        [InlineData(UploadStatus.Processing)]
        [InlineData(UploadStatus.ProcessedSuccessfully)]
        [InlineData(UploadStatus.Published)]
        [InlineData(UploadStatus.Abandoned)]
        public async Task Get_NoVenueUploadAtProcessedWithErrorsStatus_ReturnsBadRequest(UploadStatus? uploadStatus)
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var learnAimRef = await TestData.CreateLearningAimRef();

            if (uploadStatus.HasValue)
            {
                var (courseUpload, courseUploadRows) = await TestData.CreateCourseUpload(
                    provider.ProviderId,
                    createdBy: User.ToUserInfo(),
                    uploadStatus.Value);
            }

            var rowNumber = 2;

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload/courses/resolve/{rowNumber}/description?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Get_RowDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var learnAimRef = await TestData.CreateLearningAimRef();

            var (courseUpload, courseUploadRows) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors,
                configureRows: rowBuilder =>
                {
                    rowBuilder.AddRow(learnAimRef, record =>
                    {
                        record.IsValid = false;
                        record.Errors = new[] { "COURSE_WHO_THIS_COURSE_IS_FOR_REQUIRED" };
                        record.WhoThisCourseIsFor = string.Empty;
                    });
                });

            var rowNumber = courseUploadRows.Last().RowNumber + 1;

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload/courses/resolve/{rowNumber}/description?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
        
        [Fact]
        public async Task Get_RowDoesNotHaveErrors_ReturnsBadRequest()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var learnAimRef = await TestData.CreateLearningAimRef();

            var (courseUpload, courseUploadRows) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors,
                configureRows: rowBuilder =>
                {
                    rowBuilder.AddRow(learnAimRef, record =>
                    {
                        record.IsValid = false;
                        record.Errors = new[] { "COURSE_WHO_THIS_COURSE_IS_FOR_REQUIRED" };
                        record.WhoThisCourseIsFor = string.Empty;
                    });

                    rowBuilder.AddValidRow(learnAimRef);
                });

            var rowNumber = courseUploadRows.Last().RowNumber;

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload/courses/resolve/{rowNumber}/description?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Get_ValidRequest_RendersExpectedContent()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var learnAimRef = await TestData.CreateLearningAimRef();

            var (courseUpload, courseUploadRows) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors,
                configureRows: rowBuilder =>
                {
                    rowBuilder.AddRow(learnAimRef, record =>
                    {
                        record.IsValid = false;
                        record.Errors = new[] { "COURSE_WHO_THIS_COURSE_IS_FOR_REQUIRED" };
                        record.WhoThisCourseIsFor = string.Empty;
                    });
                });

            var rowNumber = courseUploadRows.First().RowNumber;

            var request = new HttpRequestMessage(HttpMethod.Get, $"/data-upload/courses/resolve/{rowNumber}/description?providerId={provider.ProviderId}");

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var doc = await response.GetDocument();
            doc.AssertHasError("WhoThisCourseIsFor", ErrorRegistry.All["COURSE_WHO_THIS_COURSE_IS_FOR_REQUIRED"].GetMessage());
        }

        [Theory]
        [InlineData(null)]
        [InlineData(UploadStatus.Created)]
        [InlineData(UploadStatus.Processing)]
        [InlineData(UploadStatus.ProcessedSuccessfully)]
        [InlineData(UploadStatus.Published)]
        [InlineData(UploadStatus.Abandoned)]
        public async Task Post_NoVenueUploadAtProcessedWithErrorsStatus_ReturnsBadRequest(UploadStatus? uploadStatus)
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var learnAimRef = await TestData.CreateLearningAimRef();

            if (uploadStatus != null)
            {
                var (courseUpload, courseUploadRows) = await TestData.CreateCourseUpload(
                    provider.ProviderId,
                    createdBy: User.ToUserInfo(),
                    uploadStatus.Value);
            }

            var rowNumber = 2;

            var whoThisCourseIsFor = "Updated who this course is for";
            var entryRequirements = "Updated entry requirements";
            var whatYouWillLearn = "Updated what you'll learn";
            var howYouWillLearn = "Updated how you'll learn";
            var whatYouWillNeedToBring = "Updated what you'll need to bring";
            var howYouWillBeAssessed = "Updated how you'll be assessed";
            var whereNext = "Updated where next";

            var request = new HttpRequestMessage(HttpMethod.Post, $"/data-upload/courses/resolve/{rowNumber}/description?providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("WhoThisCourseIsFor", whoThisCourseIsFor)
                    .Add("EntryRequirements", entryRequirements)
                    .Add("WhatYouWillLearn", whatYouWillLearn)
                    .Add("HowYouWillLearn", howYouWillLearn)
                    .Add("WhatYouWillNeedToBring", whatYouWillNeedToBring)
                    .Add("HowYouWillBeAssessed", howYouWillBeAssessed)
                    .Add("WhereNext", whereNext)
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Post_RowDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var learnAimRef = await TestData.CreateLearningAimRef();

            var (courseUpload, courseUploadRows) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors,
                configureRows: rowBuilder =>
                {
                    rowBuilder.AddRow(learnAimRef, record =>
                    {
                        record.IsValid = false;
                        record.Errors = new[] { "COURSE_WHO_THIS_COURSE_IS_FOR_REQUIRED" };
                        record.WhoThisCourseIsFor = string.Empty;
                    });
                });

            var rowNumber = courseUploadRows.Last().RowNumber + 1;

            var whoThisCourseIsFor = "Updated who this course is for";
            var entryRequirements = "Updated entry requirements";
            var whatYouWillLearn = "Updated what you'll learn";
            var howYouWillLearn = "Updated how you'll learn";
            var whatYouWillNeedToBring = "Updated what you'll need to bring";
            var howYouWillBeAssessed = "Updated how you'll be assessed";
            var whereNext = "Updated where next";

            var request = new HttpRequestMessage(HttpMethod.Post, $"/data-upload/courses/resolve/{rowNumber}/description?providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("WhoThisCourseIsFor", whoThisCourseIsFor)
                    .Add("EntryRequirements", entryRequirements)
                    .Add("WhatYouWillLearn", whatYouWillLearn)
                    .Add("HowYouWillLearn", howYouWillLearn)
                    .Add("WhatYouWillNeedToBring", whatYouWillNeedToBring)
                    .Add("HowYouWillBeAssessed", howYouWillBeAssessed)
                    .Add("WhereNext", whereNext)
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Post_RowDoesNotHaveErrors_ReturnsBadRequest()
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var learnAimRef = await TestData.CreateLearningAimRef();

            var (courseUpload, courseUploadRows) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors,
                configureRows: rowBuilder =>
                {
                    rowBuilder.AddValidRow(learnAimRef);
                });

            var rowNumber = courseUploadRows.First().RowNumber;

            var whoThisCourseIsFor = "Updated who this course is for";
            var entryRequirements = "Updated entry requirements";
            var whatYouWillLearn = "Updated what you'll learn";
            var howYouWillLearn = "Updated how you'll learn";
            var whatYouWillNeedToBring = "Updated what you'll need to bring";
            var howYouWillBeAssessed = "Updated how you'll be assessed";
            var whereNext = "Updated where next";

            var request = new HttpRequestMessage(HttpMethod.Post, $"/data-upload/courses/resolve/{rowNumber}/description?providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("WhoThisCourseIsFor", whoThisCourseIsFor)
                    .Add("EntryRequirements", entryRequirements)
                    .Add("WhatYouWillLearn", whatYouWillLearn)
                    .Add("HowYouWillLearn", howYouWillLearn)
                    .Add("WhatYouWillNeedToBring", whatYouWillNeedToBring)
                    .Add("HowYouWillBeAssessed", howYouWillBeAssessed)
                    .Add("WhereNext", whereNext)
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Theory]
        [MemberData(nameof(InvalidFixData))]
        public async Task Post_WithErrors_RendersExpectedError(
            string whoThisCourseIsFor,
            string entryRequirements,
            string whatYouWillLearn,
            string howYouWillLearn,
            string whatYouWillNeedToBring,
            string howYouWillBeAssessed,
            string whereNext,
            string expectedErrorInputId,
            string expectedErrorMessage)
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var learnAimRef = await TestData.CreateLearningAimRef();

            var (courseUpload, courseUploadRows) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors,
                configureRows: rowBuilder =>
                {
                    rowBuilder.AddRow(learnAimRef, record =>
                    {
                        record.IsValid = false;
                        record.Errors = new[] { "COURSE_WHO_THIS_COURSE_IS_FOR_REQUIRED" };
                        record.WhoThisCourseIsFor = string.Empty;
                    });
                });

            var rowNumber = courseUploadRows.First().RowNumber;

            var request = new HttpRequestMessage(HttpMethod.Post, $"/data-upload/courses/resolve/{rowNumber}/description?providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("WhoThisCourseIsFor", whoThisCourseIsFor)
                    .Add("EntryRequirements", entryRequirements)
                    .Add("WhatYouWillLearn", whatYouWillLearn)
                    .Add("HowYouWillLearn", howYouWillLearn)
                    .Add("WhatYouWillNeedToBring", whatYouWillNeedToBring) //TODO error
                    .Add("HowYouWillBeAssessed", howYouWillBeAssessed)
                    .Add("WhereNext", whereNext) //TODO error
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var doc = await response.GetDocument();
            doc.AssertHasError(expectedErrorInputId, expectedErrorMessage);
        }

        [Theory]
        [InlineData(true, "/data-upload/courses/resolve?providerId={0}")]
        [InlineData(false, "/data-upload/courses/check-publish?providerId={0}")]
        public async Task Post_ValidRequest_UpdatesRowsAndRedirects(bool otherRowsHaveErrors, string expectedLocation)
        {
            // Arrange
            var provider = await TestData.CreateProvider();

            var learnAimRef = await TestData.CreateLearningAimRef();

            UpsertCourseUploadRowsRecord thirdUploadRow = default;

            var (courseUpload, courseUploadRows) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                createdBy: User.ToUserInfo(),
                UploadStatus.ProcessedWithErrors,
                configureRows: rowBuilder =>
                {
                    // Configure 3 rows - 2 with the same course ID (that we are fixing)
                    // and one more with a different course ID.
                    // Both records with the same course ID should be updated, the other should not be.

                    var courseId1 = Guid.NewGuid();

                    rowBuilder.AddRow(learnAimRef, record =>
                    {
                        record.CourseId = courseId1;
                        record.IsValid = false;
                        record.Errors = new[] { "COURSE_WHO_THIS_COURSE_IS_FOR_REQUIRED" };
                        record.WhoThisCourseIsFor = string.Empty;
                    });

                    rowBuilder.AddRow(learnAimRef, record =>
                    {
                        record.CourseId = courseId1;
                        record.IsValid = false;
                        record.Errors = new[] { "COURSE_WHO_THIS_COURSE_IS_FOR_REQUIRED" };
                        record.WhoThisCourseIsFor = string.Empty;
                    });

                    var courseId2 = Guid.NewGuid();

                    rowBuilder.AddRow(learnAimRef, record =>
                    {
                        record.CourseId = courseId2;

                        if (otherRowsHaveErrors)
                        {
                            record.IsValid = false;
                            record.Errors = new[] { "COURSE_WHO_THIS_COURSE_IS_FOR_REQUIRED" };
                            record.WhoThisCourseIsFor = string.Empty;
                        }

                        thirdUploadRow = record;
                    });
                });

            var rowNumber = courseUploadRows.First().RowNumber;

            var whoThisCourseIsFor = "Updated who this course is for";
            var entryRequirements = "Updated entry requirements";
            var whatYouWillLearn = "Updated what you'll learn";
            var howYouWillLearn = "Updated how you'll learn";
            var whatYouWillNeedToBring = "Updated what you'll need to bring";
            var howYouWillBeAssessed = "Updated how you'll be assessed";
            var whereNext = "Updated where next";

            var request = new HttpRequestMessage(HttpMethod.Post, $"/data-upload/courses/resolve/{rowNumber}/description?providerId={provider.ProviderId}")
            {
                Content = new FormUrlEncodedContentBuilder()
                    .Add("WhoThisCourseIsFor", whoThisCourseIsFor)
                    .Add("EntryRequirements", entryRequirements)
                    .Add("WhatYouWillLearn", whatYouWillLearn)
                    .Add("HowYouWillLearn", howYouWillLearn)
                    .Add("WhatYouWillNeedToBring", whatYouWillNeedToBring)
                    .Add("HowYouWillBeAssessed", howYouWillBeAssessed)
                    .Add("WhereNext", whereNext)
                    .ToContent()
            };

            // Act
            var response = await HttpClient.SendAsync(request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location.OriginalString.Should().Be(string.Format(expectedLocation, provider.ProviderId));

            var rows = await WithSqlQueryDispatcher(
                dispatcher => dispatcher.ExecuteQuery(new GetCourseUploadRows() { CourseUploadId = courseUpload.CourseUploadId }));

            Assert.Collection(
                rows,
                row =>
                {
                    row.WhoThisCourseIsFor.Should().Be(whoThisCourseIsFor);
                    row.EntryRequirements.Should().Be(entryRequirements);
                    row.WhatYouWillLearn.Should().Be(whatYouWillLearn);
                    row.HowYouWillLearn.Should().Be(howYouWillLearn);
                    row.WhatYouWillNeedToBring.Should().Be(whatYouWillNeedToBring);
                    row.HowYouWillBeAssessed.Should().Be(howYouWillBeAssessed);
                    row.WhereNext.Should().Be(whereNext);
                    row.IsValid.Should().BeTrue();
                    row.Errors.Should().BeEmpty();
                },
                row =>
                {
                    row.WhoThisCourseIsFor.Should().Be(whoThisCourseIsFor);
                    row.EntryRequirements.Should().Be(entryRequirements);
                    row.WhatYouWillLearn.Should().Be(whatYouWillLearn);
                    row.HowYouWillLearn.Should().Be(howYouWillLearn);
                    row.WhatYouWillNeedToBring.Should().Be(whatYouWillNeedToBring);
                    row.HowYouWillBeAssessed.Should().Be(howYouWillBeAssessed);
                    row.WhereNext.Should().Be(whereNext);
                    row.IsValid.Should().BeTrue();
                    row.Errors.Should().BeEmpty();
                },
                row =>
                {
                    row.WhoThisCourseIsFor.Should().Be(thirdUploadRow.WhoThisCourseIsFor);
                    row.EntryRequirements.Should().Be(thirdUploadRow.EntryRequirements);
                    row.WhatYouWillLearn.Should().Be(thirdUploadRow.WhatYouWillLearn);
                    row.HowYouWillLearn.Should().Be(thirdUploadRow.HowYouWillLearn);
                    row.WhatYouWillNeedToBring.Should().Be(thirdUploadRow.WhatYouWillNeedToBring);
                    row.HowYouWillBeAssessed.Should().Be(thirdUploadRow.HowYouWillBeAssessed);
                    row.WhereNext.Should().Be(thirdUploadRow.WhereNext);
                });
        }

        public static IEnumerable<object[]> InvalidFixData { get; } =
            new[]
            {
                // Who this course is for missing
                (
                    WhoThisCourseIsFor: "",
                    EntryRequirements: "",
                    WhatYouWillLearn: "",
                    HowYouWillLearn: "",
                    WhatYouWillNeedToBring: "",
                    HowYouWillBeAssessed: "",
                    WhereNext: "",
                    ExpectedErrorInputId: "WhoThisCourseIsFor",
                    ExpectedErrorMessage: ErrorRegistry.All["COURSE_WHO_THIS_COURSE_IS_FOR_REQUIRED"]
                ),
                // Who this course is for is too long
                (
                    WhoThisCourseIsFor: new string('x', 2001),
                    EntryRequirements: "",
                    WhatYouWillLearn: "",
                    HowYouWillLearn: "",
                    WhatYouWillNeedToBring: "",
                    HowYouWillBeAssessed: "",
                    WhereNext: "",
                    ExpectedErrorInputId: "WhoThisCourseIsFor",
                    ExpectedErrorMessage: ErrorRegistry.All["COURSE_WHO_THIS_COURSE_IS_FOR_MAXLENGTH"]
                ),
                // Entry requirements too long
                (
                    WhoThisCourseIsFor: "Who this course is for",
                    EntryRequirements: new string('x', 501),
                    WhatYouWillLearn: "",
                    HowYouWillLearn: "",
                    WhatYouWillNeedToBring: "",
                    HowYouWillBeAssessed: "",
                    WhereNext: "",
                    ExpectedErrorInputId: "EntryRequirements",
                    ExpectedErrorMessage: ErrorRegistry.All["COURSE_ENTRY_REQUIREMENTS_MAXLENGTH"]
                ),
                // What you'll learn too long
                (
                    WhoThisCourseIsFor: "Who this course is for",
                    EntryRequirements: "",
                    WhatYouWillLearn: new string('x', 501),
                    HowYouWillLearn: "",
                    WhatYouWillNeedToBring: "",
                    HowYouWillBeAssessed: "",
                    WhereNext: "",
                    ExpectedErrorInputId: "WhatYouWillLearn",
                    ExpectedErrorMessage: ErrorRegistry.All["COURSE_WHAT_YOU_WILL_LEARN_MAXLENGTH"]
                ),
                // How you'll learn too long
                (
                    WhoThisCourseIsFor: "Who this course is for",
                    EntryRequirements: "",
                    WhatYouWillLearn: "",
                    HowYouWillLearn: new string('x', 501),
                    WhatYouWillNeedToBring: "",
                    HowYouWillBeAssessed: "",
                    WhereNext: "",
                    ExpectedErrorInputId: "HowYouWillLearn",
                    ExpectedErrorMessage: ErrorRegistry.All["COURSE_HOW_YOU_WILL_LEARN_MAXLENGTH"]
                ),
                // What you'll need to bring too long
                (
                    WhoThisCourseIsFor: "Who this course is for",
                    EntryRequirements: "",
                    WhatYouWillLearn: "",
                    HowYouWillLearn: "",
                    WhatYouWillNeedToBring: new string('x', 501),
                    HowYouWillBeAssessed: "",
                    WhereNext: "",
                    ExpectedErrorInputId: "WhatYouWillNeedToBring",
                    ExpectedErrorMessage: ErrorRegistry.All["COURSE_WHAT_YOU_WILL_NEED_TO_BRING_MAXLENGTH"]
                ),
                // How you'll be assessed too long
                (
                    WhoThisCourseIsFor: "Who this course is for",
                    EntryRequirements: "",
                    WhatYouWillLearn: "",
                    HowYouWillLearn: "",
                    WhatYouWillNeedToBring: "",
                    HowYouWillBeAssessed: new string('x', 501),
                    WhereNext: "",
                    ExpectedErrorInputId: "HowYouWillBeAssessed",
                    ExpectedErrorMessage: ErrorRegistry.All["COURSE_HOW_YOU_WILL_BE_ASSESSED_MAXLENGTH"]
                ),
                // Where next too long
                (
                    WhoThisCourseIsFor: "Who this course is for",
                    EntryRequirements: "",
                    WhatYouWillLearn: "",
                    HowYouWillLearn: "",
                    WhatYouWillNeedToBring: "",
                    HowYouWillBeAssessed: "",
                    WhereNext: new string('x', 501),
                    ExpectedErrorInputId: "WhereNext",
                    ExpectedErrorMessage: ErrorRegistry.All["COURSE_WHERE_NEXT_MAXLENGTH"]
                ),
            }
            .Select(t => new object[]
            {
                t.WhoThisCourseIsFor,
                t.EntryRequirements,
                t.WhatYouWillLearn,
                t.HowYouWillLearn,
                t.WhatYouWillNeedToBring,
                t.HowYouWillBeAssessed,
                t.WhereNext,
                t.ExpectedErrorInputId,
                t.ExpectedErrorMessage
            });
    }
}
