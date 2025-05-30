﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataManagement;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Testing;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace Dfc.CourseDirectory.Core.Tests.DataManagementTests
{
    public partial class FileUploadProcessorTests
    {
        [Fact]
        public async Task FindVenue_RowHasVenueIdHint_ReturnsVenueMatchingHint()
        {
            // Arrange
            var fileUploadProcessor = this.CreateUploadProcessor();

            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var learnAimRef = (await TestData.CreateLearningDelivery()).LearnAimRef;
            var venue = await TestData.CreateVenue(providerId: provider.ProviderId, createdBy: user, venueName: "My Venue", providerVenueRef: "VENUE1");

            var row = new CsvCourseRow()
            {
                DeliveryMode = "classroom based",
                ProviderVenueRef = "xxx"
            };

            var rowInfo = new CourseDataUploadRowInfo(row, rowNumber: 2, courseId: Guid.NewGuid(), venueIdHint: venue.VenueId);

            // Act
            var result = fileUploadProcessor.FindVenue(rowInfo.VenueIdHint, rowInfo.Data.VenueName, rowInfo.Data.ProviderVenueRef, new[] { venue });

            // Assert
            Assert.Equal(venue.VenueId, result?.VenueId);
        }

        [Fact]
        public async Task FindVenue_RefProvidedButDoesNotMatch_ReturnsNull()
        {
            // Arrange
            var fileUploadProcessor = this.CreateUploadProcessor();

            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var venue = await TestData.CreateVenue(providerId: provider.ProviderId, createdBy: user, venueName: "My Venue", providerVenueRef: "VENUE1");

            var row = new CsvCourseRow()
            {
                DeliveryMode = "classroom based",
                ProviderVenueRef = "VENUE2"
            };

            var rowInfo = new CourseDataUploadRowInfo(row, rowNumber: 2, courseId: Guid.NewGuid());

            // Act
            var result = fileUploadProcessor.FindVenue(rowInfo.VenueIdHint, rowInfo.Data.VenueName, rowInfo.Data.ProviderVenueRef, new[] { venue });

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task FindVenue_RefProvidedAndMatches_ReturnsVenueId()
        {
            // Arrange
            var fileUploadProcessor = this.CreateUploadProcessor();

            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var venue = await TestData.CreateVenue(providerId: provider.ProviderId, createdBy: user, venueName: "My Venue", providerVenueRef: "VENUE1");

            var row = new CsvCourseRow()
            {
                DeliveryMode = "classroom based",
                ProviderVenueRef = "venue1"
            };

            var rowInfo = new CourseDataUploadRowInfo(row, rowNumber: 2, courseId: Guid.NewGuid());

            // Act
            var result = fileUploadProcessor.FindVenue(rowInfo.VenueIdHint, rowInfo.Data.VenueName, rowInfo.Data.ProviderVenueRef, new[] { venue });

            // Assert
            Assert.Equal(venue.VenueId, result?.VenueId);
        }

        [Fact]
        public async Task FindVenue_RefProvidedAndMatchedButNameProvidedAndDoesNotMatch_ReturnsNull()
        {
            // Arrange
            var fileUploadProcessor = this.CreateUploadProcessor();

            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var venue = await TestData.CreateVenue(providerId: provider.ProviderId, createdBy: user, venueName: "My Venue", providerVenueRef: "VENUE1");

            var row = new CsvCourseRow()
            {
                DeliveryMode = "classroom based",
                ProviderVenueRef = "venue1",
                VenueName = "another name"
            };

            var rowInfo = new CourseDataUploadRowInfo(row, rowNumber: 2, courseId: Guid.NewGuid());

            // Act
            var result = fileUploadProcessor.FindVenue(rowInfo.VenueIdHint, rowInfo.Data.VenueName, rowInfo.Data.ProviderVenueRef, new[] { venue });

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task FindVenue_RefProvidedAndMatchedAndNameProvidedAndMatches_ReturnsVenueId()
        {
            // Arrange
            var fileUploadProcessor = this.CreateUploadProcessor();

            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var venue = await TestData.CreateVenue(providerId: provider.ProviderId, createdBy: user, venueName: "My Venue", providerVenueRef: "VENUE1");

            var row = new CsvCourseRow()
            {
                DeliveryMode = "classroom based",
                ProviderVenueRef = "venue1",
                VenueName = "my venue"
            };

            var rowInfo = new CourseDataUploadRowInfo(row, rowNumber: 2, courseId: Guid.NewGuid());

            // Act
            var result = fileUploadProcessor.FindVenue(rowInfo.VenueIdHint, rowInfo.Data.VenueName, rowInfo.Data.ProviderVenueRef, new[] { venue });

            // Assert
            Assert.Equal(venue.VenueId, result?.VenueId);
        }

        [Fact]
        public async Task FindVenue_NameProvidedButDoesNotMatch_ReturnsNull()
        {
            // Arrange
            var fileUploadProcessor = this.CreateUploadProcessor();

            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var venue = await TestData.CreateVenue(providerId: provider.ProviderId, createdBy: user, venueName: "My Venue", providerVenueRef: "VENUE1");

            var row = new CsvCourseRow()
            {
                DeliveryMode = "classroom based",
                VenueName = "another name"
            };

            var rowInfo = new CourseDataUploadRowInfo(row, rowNumber: 2, courseId: Guid.NewGuid());

            // Act
            var result = fileUploadProcessor.FindVenue(rowInfo.VenueIdHint, rowInfo.Data.VenueName, rowInfo.Data.ProviderVenueRef, new[] { venue });

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task FindVenue_NameProvidedAndMatches_ReturnsVenueId()
        {
            // Arrange
            var fileUploadProcessor = this.CreateUploadProcessor();

            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var venue = await TestData.CreateVenue(providerId: provider.ProviderId, createdBy: user, venueName: "My Venue", providerVenueRef: "VENUE1");

            var row = new CsvCourseRow()
            {
                DeliveryMode = "classroom based",
                VenueName = "my venue"
            };

            var rowInfo = new CourseDataUploadRowInfo(row, rowNumber: 2, courseId: Guid.NewGuid());

            // Act
            var result = fileUploadProcessor.FindVenue(rowInfo.VenueIdHint, rowInfo.Data.VenueName, rowInfo.Data.ProviderVenueRef, new[] { venue });

            // Assert
            Assert.Equal(venue.VenueId, result?.VenueId);
        }

        [Fact]
        public async Task GetCourseUploadRowsRequiringRevalidation_MatchedVenueUpdatedSinceRowLastValidated_ReturnsRow()
        {
            // Arrange
            var fileUploadProcessor = this.CreateUploadProcessor();

            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var learnAimRef = (await TestData.CreateLearningDelivery()).LearnAimRef;

            var venue = await TestData.CreateVenue(provider.ProviderId, createdBy: user, venueName: "My Venue", providerVenueRef: "REF");

            var (courseUpload, rows) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                user,
                UploadStatus.ProcessedWithErrors,
                configureRows: rowBuilder =>
                {
                    rowBuilder.AddRow(learnAimRef, record =>
                    {
                        record.DeliveryMode = "classroom based";
                        record.ResolvedDeliveryMode = CourseDeliveryMode.ClassroomBased;
                        record.ProviderVenueRef = venue.ProviderVenueRef;
                        record.VenueId = venue.VenueId;
                    });
                });

            Clock.UtcNow += TimeSpan.FromHours(1);
            await TestData.UpdateVenue(venue.VenueId, updatedBy: user);

            // Act
            var result = await WithSqlQueryDispatcher(
                dispatcher => fileUploadProcessor.GetCourseUploadRowsRequiringRevalidation(dispatcher, courseUpload));

            // Assert
            Assert.Collection(
                result,
                row => Assert.Equal(rows[0].CourseRunId, row.CourseRunId));
        }

        [Fact]
        public async Task GetCourseUploadRowsRequiringRevalidation_MatchedVenueNotUpdatedSinceRowLastValidated_DoesNotReturnRow()
        {
            // Arrange
            var fileUploadProcessor = this.CreateUploadProcessor();

            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var learnAimRef = (await TestData.CreateLearningDelivery()).LearnAimRef;

            var venue = await TestData.CreateVenue(provider.ProviderId, createdBy: user, venueName: "My Venue", providerVenueRef: "REF");

            var (courseUpload, rows) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                user,
                UploadStatus.ProcessedWithErrors,
                configureRows: rowBuilder =>
                {
                    rowBuilder.AddRow(learnAimRef, record =>
                    {
                        record.DeliveryMode = "classroom based";
                        record.ResolvedDeliveryMode = CourseDeliveryMode.ClassroomBased;
                        record.ProviderVenueRef = venue.ProviderVenueRef;
                        record.VenueId = venue.VenueId;
                    });
                });

            // Act
            var result = await WithSqlQueryDispatcher(
                dispatcher => fileUploadProcessor.GetCourseUploadRowsRequiringRevalidation(dispatcher, courseUpload));

            // Assert
            Assert.Collection(result);
        }

        [Fact]
        public async Task GetCourseUploadRowsRequiringRevalidation_MatchedVenueNotUpdatedSinceRowLastValidatedButOtherVenuesAmended_DoesNotReturnRow()
        {
            // Arrange
            var fileUploadProcessor = this.CreateUploadProcessor();

            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var learnAimRef = (await TestData.CreateLearningDelivery()).LearnAimRef;

            var venue = await TestData.CreateVenue(provider.ProviderId, createdBy: user, venueName: "My Venue", providerVenueRef: "REF");

            var (courseUpload, rows) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                user,
                UploadStatus.ProcessedWithErrors,
                configureRows: rowBuilder =>
                {
                    rowBuilder.AddRow(learnAimRef, record =>
                    {
                        record.DeliveryMode = "classroom based";
                        record.ResolvedDeliveryMode = CourseDeliveryMode.ClassroomBased;
                        record.ProviderVenueRef = venue.ProviderVenueRef;
                        record.VenueId = venue.VenueId;
                    });
                });

            Clock.UtcNow += TimeSpan.FromHours(1);
            await TestData.CreateVenue(provider.ProviderId, createdBy: user, venueName: "Another Venue");

            // Act
            var result = await WithSqlQueryDispatcher(
                dispatcher => fileUploadProcessor.GetCourseUploadRowsRequiringRevalidation(dispatcher, courseUpload));

            // Assert
            Assert.Collection(result);
        }

        [Fact]
        public async Task GetCourseUploadRowsRequiringRevalidation_NoMatchedVenueAndProviderVenuesAmendedSinceRowLastValidated_ReturnsRow()
        {
            // Arrange
            var fileUploadProcessor = this.CreateUploadProcessor();

            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var learnAimRef = (await TestData.CreateLearningDelivery()).LearnAimRef;

            var (courseUpload, rows) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                user,
                UploadStatus.ProcessedWithErrors,
                configureRows: rowBuilder =>
                {
                    rowBuilder.AddRow(learnAimRef, record =>
                    {
                        record.DeliveryMode = "classroom based";
                        record.ResolvedDeliveryMode = CourseDeliveryMode.ClassroomBased;
                        record.ProviderVenueRef = "REF";
                    });
                });

            Clock.UtcNow += TimeSpan.FromHours(1);
            await TestData.CreateVenue(provider.ProviderId, createdBy: user, venueName: "Another Venue");

            // Act
            var result = await WithSqlQueryDispatcher(
                dispatcher => fileUploadProcessor.GetCourseUploadRowsRequiringRevalidation(dispatcher, courseUpload));

            // Assert
            Assert.Collection(
                result,
                row => Assert.Equal(rows[0].CourseRunId, row.CourseRunId));
        }

        [Fact]
        public async Task ProcessCourseFile_AllRecordsValid_SetStatusToProcessedSuccessfully()
        {
            // Arrange
            var fileUploadProcessor = this.CreateUploadProcessor();

            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var learnAimRef = (await TestData.CreateLearningDelivery()).LearnAimRef;
            var (courseUpload, _) = await TestData.CreateCourseUpload(provider.ProviderId, user, UploadStatus.Created);

            var uploadRows = DataManagementFileHelper.CreateCourseUploadRows(learnAimRef, rowCount: 3).ToArray();
            var stream = DataManagementFileHelper.CreateCourseUploadCsvStream(uploadRows);

            // Act
            await fileUploadProcessor.ProcessCourseFile(courseUpload.CourseUploadId, stream);

            // Assert
            courseUpload = await WithSqlQueryDispatcher(
                dispatcher => dispatcher.ExecuteQuery(new GetCourseUpload() { CourseUploadId = courseUpload.CourseUploadId }));

            using (new AssertionScope())
            {
                courseUpload.UploadStatus.Should().Be(UploadStatus.ProcessedSuccessfully);
                courseUpload.ProcessingCompletedOn.Should().Be(Clock.UtcNow);
                courseUpload.ProcessingStartedOn.Should().NotBeNull();
            }
        }

        [Fact]
        public async Task ProcessNonLarsCourseFile_AllRecordsValid_SetStatusToProcessedSuccessfully()
        {
            // Arrange
            var fileUploadProcessor = this.CreateUploadProcessor();

            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);

            await TestData.AddSectors();
            await TestData.CreateNonLarsCourse(provider.ProviderId, user);

            var (courseUpload, _) = await TestData.CreateCourseUpload(provider.ProviderId, user, UploadStatus.Created, null, true);

            var uploadRows = DataManagementFileHelper.CreateNonLarsCourseUploadRows(rowCount: 3).ToArray();
            var stream = DataManagementFileHelper.CreateNonLarsCourseUploadCsvStream(uploadRows);

            // Act
            await fileUploadProcessor.ProcessCourseFile(courseUpload.CourseUploadId, stream);

            // Assert
            courseUpload = await WithSqlQueryDispatcher(
                dispatcher => dispatcher.ExecuteQuery(new GetCourseUpload() { CourseUploadId = courseUpload.CourseUploadId }));

            using (new AssertionScope())
            {
                courseUpload.UploadStatus.Should().Be(UploadStatus.ProcessedSuccessfully);
                courseUpload.ProcessingCompletedOn.Should().Be(Clock.UtcNow);
                courseUpload.ProcessingStartedOn.Should().NotBeNull();
            }
        }

        [Fact]
        public async Task ProcessNonLarsCourseFile_CorrectSectorWithLowerCase_SetStatusToProcessedSuccessfully()
        {
            // Arrange
            var fileUploadProcessor = this.CreateUploadProcessor();

            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);

            await TestData.AddSectors();
            await TestData.CreateNonLarsCourse(provider.ProviderId, user);

            var (courseUpload, _) = await TestData.CreateCourseUpload(provider.ProviderId, user, UploadStatus.Created, null, true);

            var uploadRows = DataManagementFileHelper.CreateNonLarsCourseUploadRows(rowCount: 3).ToArray();
            uploadRows[1].Sector = "environmental";
            var stream = DataManagementFileHelper.CreateNonLarsCourseUploadCsvStream(uploadRows);

            // Act
            await fileUploadProcessor.ProcessCourseFile(courseUpload.CourseUploadId, stream);

            // Assert
            courseUpload = await WithSqlQueryDispatcher(
                dispatcher => dispatcher.ExecuteQuery(new GetCourseUpload() { CourseUploadId = courseUpload.CourseUploadId }));

            using (new AssertionScope())
            {
                courseUpload.UploadStatus.Should().Be(UploadStatus.ProcessedSuccessfully);
                courseUpload.ProcessingCompletedOn.Should().Be(Clock.UtcNow);
                courseUpload.ProcessingStartedOn.Should().NotBeNull();
            }
        }

        [Fact]
        public async Task ProcessCourseFile_RowHasErrors_SetStatusToProcessedWithErrors()
        {
            // Arrange
            var fileUploadProcessor = this.CreateUploadProcessor();

            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var learnAimRef = (await TestData.CreateLearningDelivery()).LearnAimRef;
            var (courseUpload, _) = await TestData.CreateCourseUpload(provider.ProviderId, user, UploadStatus.Created);

            var stream = DataManagementFileHelper.CreateCourseUploadCsvStream(
                // Empty record will always yield errors (but we will always have a valid LARS code at this point)
                new CsvCourseRow()
                {
                    LearnAimRef = learnAimRef
                });

            // Act
            await fileUploadProcessor.ProcessCourseFile(courseUpload.CourseUploadId, stream);

            // Assert
            courseUpload = await WithSqlQueryDispatcher(
                dispatcher => dispatcher.ExecuteQuery(new GetCourseUpload() { CourseUploadId = courseUpload.CourseUploadId }));

            using (new AssertionScope())
            {
                courseUpload.UploadStatus.Should().Be(UploadStatus.ProcessedWithErrors);
                courseUpload.ProcessingCompletedOn.Should().Be(Clock.UtcNow);
                courseUpload.ProcessingStartedOn.Should().NotBeNull();
            }
        }

        [Fact]
        public async Task ProcessNonLarsCourseFile_SectorFieldIsEmpty_SetStatusToProcessedWithErrors()
        {
            // Arrange
            var fileUploadProcessor = this.CreateUploadProcessor();

            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);

            var (courseUpload, _) = await TestData.CreateCourseUpload(provider.ProviderId, user, UploadStatus.Created, null, true);

            var uploadRows = DataManagementFileHelper.CreateNonLarsCourseUploadRows(rowCount: 3).ToArray();
            uploadRows[1].Sector = "";
            var stream = DataManagementFileHelper.CreateNonLarsCourseUploadCsvStream(uploadRows);

            // Act
            await fileUploadProcessor.ProcessCourseFile(courseUpload.CourseUploadId, stream);

            // Assert
            courseUpload = await WithSqlQueryDispatcher(
                dispatcher => dispatcher.ExecuteQuery(new GetCourseUpload() { CourseUploadId = courseUpload.CourseUploadId }));

            using (new AssertionScope())
            {
                courseUpload.UploadStatus.Should().Be(UploadStatus.ProcessedWithErrors);
                courseUpload.ProcessingCompletedOn.Should().Be(Clock.UtcNow);
                courseUpload.ProcessingStartedOn.Should().NotBeNull();
            }
        }

        [Fact]
        public async Task ProcessNonLarsCourseFile_RowHasErrors_SetStatusToProcessedWithErrors()
        {
            // Arrange
            var fileUploadProcessor = this.CreateUploadProcessor();

            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);

            var (courseUpload, _) = await TestData.CreateCourseUpload(provider.ProviderId, user, UploadStatus.Created, null, true);

            var stream = DataManagementFileHelper.CreateNonLarsCourseUploadCsvStream(
                // Empty record will always yield errors
                new CsvNonLarsCourseRow()
                {
                });

            // Act
            await fileUploadProcessor.ProcessCourseFile(courseUpload.CourseUploadId, stream);

            // Assert
            courseUpload = await WithSqlQueryDispatcher(
                dispatcher => dispatcher.ExecuteQuery(new GetCourseUpload() { CourseUploadId = courseUpload.CourseUploadId }));

            using (new AssertionScope())
            {
                courseUpload.UploadStatus.Should().Be(UploadStatus.ProcessedWithErrors);
                courseUpload.ProcessingCompletedOn.Should().Be(Clock.UtcNow);
                courseUpload.ProcessingStartedOn.Should().NotBeNull();
            }
        }

        [Fact]
        public async Task ProcessCourseFile_EmptyCourseName_GetsCourseNameFromLearningDelivery()
        {
            // Arrange
            var fileUploadProcessor = this.CreateUploadProcessor();

            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var (courseUpload, _) = await TestData.CreateCourseUpload(provider.ProviderId, user, UploadStatus.Created);

            var learningDelivery = await TestData.CreateLearningDelivery(learnAimRefTitle: "LARS title");

            var uploadRows = DataManagementFileHelper.CreateCourseUploadRows(learningDelivery.LearnAimRef, rowCount: 1).ToArray();
            uploadRows[0].CourseName = string.Empty;

            var stream = DataManagementFileHelper.CreateCourseUploadCsvStream(uploadRows);

            // Act
            await fileUploadProcessor.ProcessCourseFile(courseUpload.CourseUploadId, stream);

            // Assert
            var rows = await WithSqlQueryDispatcher(async dispatcher =>
                (await dispatcher.ExecuteQuery(new GetCourseUploadRows()
                {
                    CourseUploadId = courseUpload.CourseUploadId,
                    WithErrorsOnly = false
                })).Rows);

            rows.Single().CourseName.Should().Be(learningDelivery.LearnAimRefTitle);
        }

        [Fact]
        public async Task ProcessCourseFile_LearnAimRefWithMissingLeadingZero_HasLeadingZeroAdded()
        {
            // Arrange
            var fileUploadProcessor = this.CreateUploadProcessor();

            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var (courseUpload, _) = await TestData.CreateCourseUpload(provider.ProviderId, user, UploadStatus.Created);

            var learningDelivery = await TestData.CreateLearningDelivery(learnAimRef: "01234567");

            var uploadRows = DataManagementFileHelper.CreateCourseUploadRows(learningDelivery.LearnAimRef.TrimStart('0'), rowCount: 1).ToArray();
            uploadRows[0].CourseName = string.Empty;

            var stream = DataManagementFileHelper.CreateCourseUploadCsvStream(uploadRows);

            // Act
            await fileUploadProcessor.ProcessCourseFile(courseUpload.CourseUploadId, stream);

            // Assert
            var rows = await WithSqlQueryDispatcher(async dispatcher =>
                (await dispatcher.ExecuteQuery(new GetCourseUploadRows()
                {
                    CourseUploadId = courseUpload.CourseUploadId,
                    WithErrorsOnly = false
                })).Rows);

            rows.Single().LearnAimRef.Should().Be(learningDelivery.LearnAimRef);
        }

        [Fact]
        public async Task PublishCourseUpload_StatusIsProcessedWithErrors_ReturnsUploadHasErrors()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);

            await TestData.CreateCourseUpload(
                provider.ProviderId,
                createdBy: user,
                UploadStatus.ProcessedWithErrors);

            var fileUploadProcessor = this.CreateUploadProcessor();

            // Act
            var result = await fileUploadProcessor.PublishCourseUploadForProvider(provider.ProviderId, user, false);

            // Assert
            result.Status.Should().Be(PublishResultStatus.UploadHasErrors);
        }

        [Fact]
        public async Task PublishNonLarsCourseUpload_StatusIsProcessedWithErrors_ReturnsUploadHasErrors()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);

            await TestData.CreateCourseUpload(
                provider.ProviderId,
                createdBy: user,
                UploadStatus.ProcessedWithErrors, null, true);

            var fileUploadProcessor = this.CreateUploadProcessor();

            // Act
            var result = await fileUploadProcessor.PublishCourseUploadForProvider(provider.ProviderId, user, true);

            // Assert
            result.Status.Should().Be(PublishResultStatus.UploadHasErrors);
        }

        [Fact]
        public async Task PublishCourseUpload_StatusIsProcessedWithErrorsAfterRevalidation_ReturnsUploadHasErrors()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);

            var venue = await TestData.CreateVenue(provider.ProviderId, createdBy: user, venueName: "My Venue", providerVenueRef: "VENUE1");

            var learnAimRef = (await TestData.CreateLearningDelivery()).LearnAimRef;

            var (courseUpload, _) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                createdBy: user,
                UploadStatus.ProcessedSuccessfully,
                rowBuilder =>
                {
                    rowBuilder.AddRow(learnAimRef, record =>
                    {
                        record.DeliveryMode = "classroom based";
                        record.ResolvedDeliveryMode = CourseDeliveryMode.ClassroomBased;
                        record.ProviderVenueRef = venue.ProviderVenueRef;
                        record.VenueId = venue.VenueId;
                    });
                });

            // Delete the venue linked to the row in the upload, triggering revalidation
            // (which should fail since the venue has gone away)

            Clock.UtcNow += TimeSpan.FromDays(1);

            await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(new DeleteVenue()
            {
                VenueId = venue.VenueId,
                DeletedBy = user,
                DeletedOn = Clock.UtcNow
            }));

            var fileUploadProcessor = this.CreateUploadProcessor();

            // Act
            var result = await fileUploadProcessor.PublishCourseUploadForProvider(provider.ProviderId, user, false);

            // Assert
            result.Status.Should().Be(PublishResultStatus.UploadHasErrors);
        }

        [Fact]
        public async Task PublishNonLarsCourseUpload_StatusIsProcessedWithErrorsAfterRevalidation_ReturnsUploadHasErrors()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);

            var venue = await TestData.CreateVenue(provider.ProviderId, createdBy: user, venueName: "My Venue", providerVenueRef: "VENUE1");

            var (courseUpload, _) = await TestData.CreateCourseUpload(
                provider.ProviderId,
                createdBy: user,
                UploadStatus.ProcessedSuccessfully,
                rowBuilder =>
                {
                    rowBuilder.AddNonLarsRow(record =>
                    {
                        record.CourseType = "Skills Bootcamp";
                        record.ResolvedCourseType = CourseType.SkillsBootcamp;
                        record.DeliveryMode = "classroom based";
                        record.ResolvedDeliveryMode = CourseDeliveryMode.ClassroomBased;
                        record.ProviderVenueRef = venue.ProviderVenueRef;
                        record.VenueId = venue.VenueId;
                    });
                }, true);

            // Delete the venue linked to the row in the upload, triggering revalidation
            // (which should fail since the venue has gone away)

            Clock.UtcNow += TimeSpan.FromDays(1);

            await WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(new DeleteVenue()
            {
                VenueId = venue.VenueId,
                DeletedBy = user,
                DeletedOn = Clock.UtcNow
            }));

            var fileUploadProcessor = this.CreateUploadProcessor();

            // Act
            var result = await fileUploadProcessor.PublishCourseUploadForProvider(provider.ProviderId, user, true);

            // Assert
            result.Status.Should().Be(PublishResultStatus.UploadHasErrors);
        }

        [Fact(Skip = "Awaiting courses in SQL")]
        public Task PublishCourseUpload_CanBePublished_UpsertsRowsArchivesUnmatchedVenuesAndSetsStatusToPublished()
        {
            // TODO Implement once we have courses in SQL
            throw new NotImplementedException();
        }

        [Fact]
        public async Task ValidateCourseUploadRows_RowsHaveNoLarsCode_AreNotGrouped()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var (courseUpload, _) = await TestData.CreateCourseUpload(provider.ProviderId, createdBy: user, null);
            var learnAimRef = (await TestData.CreateLearningDelivery()).LearnAimRef;

            var fileUploadProcessor = this.CreateUploadProcessor();

            var rows = DataManagementFileHelper.CreateCourseUploadRows(learnAimRef, rowCount: 2).ToArray();
            rows[0].LearnAimRef = string.Empty;
            rows[1].LearnAimRef = string.Empty;

            var uploadRows = rows.ToDataUploadRowCollection();

            await WithSqlQueryDispatcher(async dispatcher =>
            {
                // Act
                var (_, rows) = await fileUploadProcessor.ValidateCourseUploadRows(
                    dispatcher,
                    courseUpload.CourseUploadId,
                    provider.ProviderId,
                    uploadRows);

                // Assert
                rows.First().CourseId.Should().NotBe(rows.Last().CourseId);
            });
        }
        [Fact]
        public async Task ValidateNonLarsCourseUploadRows_RowsHaveNoCourseType_AreNotGrouped()
        {
            // Arrange
            var provider = await TestData.CreateProvider();
            var user = await TestData.CreateUser(providerId: provider.ProviderId);
            var (courseUpload, _) = await TestData.CreateCourseUpload(provider.ProviderId, createdBy: user, null, null, null, null, null, true);


            var fileUploadProcessor = this.CreateUploadProcessor();

            var rows = DataManagementFileHelper.CreateNonLarsCourseUploadRows(rowCount: 2).ToArray();
            rows[0].CourseType = string.Empty;
            rows[1].CourseType = string.Empty;

            var uploadRows = rows.ToDataUploadRowCollection();

            await WithSqlQueryDispatcher(async dispatcher =>
            {
                // Act
                var (_, rows) = await fileUploadProcessor.ValidateNonLarsCourseUploadRows(
                    dispatcher,
                    courseUpload.CourseUploadId,
                    provider.ProviderId,
                    uploadRows);

                // Assert
                rows.First().CourseId.Should().NotBe(rows.Last().CourseId);
            });
        }
    }
}
