using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Validation.CourseValidation;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataManagement
{
    public partial class FileUploadProcessor
    {
        public async Task DeleteCourseUploadForProvider(Guid providerId, bool isNonLars)
        {
            using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher(System.Data.IsolationLevel.ReadCommitted))
            {
                await AcquireExclusiveCourseUploadLockForProvider(providerId, dispatcher);

                var courseUpload = await dispatcher.ExecuteQuery(new GetLatestUnpublishedCourseUploadForProvider()
                {
                    ProviderId = providerId,
                    IsNonLars = isNonLars
                });

                if (courseUpload == null)
                {
                    throw new InvalidStateException(InvalidStateReason.NoUnpublishedCourseUpload);
                }

                if (courseUpload.UploadStatus != UploadStatus.ProcessedWithErrors &&
                    courseUpload.UploadStatus != UploadStatus.ProcessedSuccessfully)
                {
                    throw new InvalidUploadStatusException(
                        courseUpload.UploadStatus,
                        UploadStatus.ProcessedWithErrors,
                        UploadStatus.ProcessedSuccessfully);
                }

                await dispatcher.ExecuteQuery(
                    new SetCourseUploadAbandoned()
                    {
                        CourseUploadId = courseUpload.CourseUploadId,
                        AbandonedOn = _clock.UtcNow
                    });

                await dispatcher.Commit();
            }
        }

        public async Task<UploadStatus> DeleteCourseUploadRowForProvider(Guid providerId, int rowNumber, bool isNonLars)
        {
            using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher(System.Data.IsolationLevel.ReadCommitted))
            {
                await AcquireExclusiveCourseUploadLockForProvider(providerId, dispatcher);

                var courseUpload = await dispatcher.ExecuteQuery(new GetLatestUnpublishedCourseUploadForProvider()
                {
                    ProviderId = providerId,
                    IsNonLars = isNonLars
                });

                if (courseUpload == null)
                {
                    throw new InvalidStateException(InvalidStateReason.NoUnpublishedCourseUpload);
                }

                var result = await dispatcher.ExecuteQuery(new DeleteCourseUploadRow()
                {
                    CourseUploadId = courseUpload.CourseUploadId,
                    RowNumber = rowNumber
                });

                if (result.Value is NotFound)
                {
                    throw new ResourceDoesNotExistException(ResourceType.CourseUploadRow, rowNumber);
                }

                var uploadStatus = await RefreshCourseUploadValidationStatus(courseUpload.CourseUploadId, dispatcher);

                await dispatcher.Commit();

                return uploadStatus;
            }
        }

        public async Task<UploadStatus> DeleteCourseUploadRowGroupForProvider(Guid providerId, Guid courseId, bool isNonLars)
        {
            using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher(System.Data.IsolationLevel.ReadCommitted))
            {
                await AcquireExclusiveCourseUploadLockForProvider(providerId, dispatcher);

                var courseUpload = await dispatcher.ExecuteQuery(new GetLatestUnpublishedCourseUploadForProvider()
                {
                    ProviderId = providerId,
                    IsNonLars = isNonLars
                });

                if (courseUpload == null)
                {
                    throw new InvalidStateException(InvalidStateReason.NoUnpublishedCourseUpload);
                }

                var deleteResult = await dispatcher.ExecuteQuery(new DeleteCourseUploadRowGroup()
                {
                    CourseUploadId = courseUpload.CourseUploadId,
                    CourseId = courseId
                });

                if (deleteResult.Value is NotFound)
                {
                    throw new ResourceDoesNotExistException(ResourceType.CourseUploadRowGroup, courseId);
                }

                var uploadStatus = await RefreshCourseUploadValidationStatus(courseUpload.CourseUploadId, dispatcher);

                await dispatcher.Commit();

                return uploadStatus;
            }
        }

        public async Task<CourseUploadRowDetail> GetCourseUploadRowDetailForProvider(Guid providerId, int rowNumber, bool isNonLars)
        {
            using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher(System.Data.IsolationLevel.ReadCommitted))
            {
                await AcquireExclusiveCourseUploadLockForProvider(providerId, dispatcher);

                var courseUpload = await dispatcher.ExecuteQuery(new GetLatestUnpublishedCourseUploadForProvider()
                {
                    ProviderId = providerId,
                    IsNonLars = isNonLars
                });

                if (courseUpload == null)
                {
                    throw new InvalidStateException(InvalidStateReason.NoUnpublishedCourseUpload);
                }

                if (courseUpload.UploadStatus != UploadStatus.ProcessedSuccessfully &&
                    courseUpload.UploadStatus != UploadStatus.ProcessedWithErrors)
                {
                    throw new InvalidUploadStatusException(
                        courseUpload.UploadStatus,
                        UploadStatus.ProcessedSuccessfully,
                        UploadStatus.ProcessedWithErrors);
                }

                // If the world around us has changed (courses added etc.) then we might need to revalidate
                await RevalidateCourseUploadIfRequired(dispatcher, courseUpload.CourseUploadId, isNonLars);

                var row = await dispatcher.ExecuteQuery(new GetCourseUploadRowDetail()
                {
                    CourseUploadId = courseUpload.CourseUploadId,
                    RowNumber = rowNumber
                });

                await dispatcher.Commit();

                return row;
            }
        }

        public async Task<IReadOnlyCollection<CourseUploadRow>> GetCourseUploadRowGroupForProvider(Guid providerId, Guid courseId, bool isNonLars)
        {
            using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher(System.Data.IsolationLevel.ReadCommitted))
            {
                await AcquireExclusiveCourseUploadLockForProvider(providerId, dispatcher);

                var courseUpload = await dispatcher.ExecuteQuery(new GetLatestUnpublishedCourseUploadForProvider()
                {
                    ProviderId = providerId,
                    IsNonLars = isNonLars
                });

                if (courseUpload == null)
                {
                    throw new InvalidStateException(InvalidStateReason.NoUnpublishedCourseUpload);
                }

                if (courseUpload.UploadStatus != UploadStatus.ProcessedSuccessfully &&
                    courseUpload.UploadStatus != UploadStatus.ProcessedWithErrors)
                {
                    throw new InvalidUploadStatusException(
                        courseUpload.UploadStatus,
                        UploadStatus.ProcessedSuccessfully,
                        UploadStatus.ProcessedWithErrors);
                }

                // If the world around us has changed (courses added etc.) then we might need to revalidate
                await RevalidateCourseUploadIfRequired(dispatcher, courseUpload.CourseUploadId, false);

                var rows = await dispatcher.ExecuteQuery(new GetCourseUploadRowsByCourseId()
                {
                    CourseUploadId = courseUpload.CourseUploadId,
                    CourseId = courseId
                });

                await dispatcher.Commit();

                return rows;
            }
        }

        public async Task<(IReadOnlyCollection<CourseUploadRow> Rows, UploadStatus UploadStatus)> GetCourseUploadRowsForProvider(Guid providerId, bool isNonLars)
        {
            using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher(System.Data.IsolationLevel.ReadCommitted))
            {
                await AcquireExclusiveCourseUploadLockForProvider(providerId, dispatcher);

                var courseUpload = await dispatcher.ExecuteQuery(new GetLatestUnpublishedCourseUploadForProvider()
                {
                    ProviderId = providerId,
                    IsNonLars = isNonLars
                });

                if (courseUpload == null)
                {
                    throw new InvalidStateException(InvalidStateReason.NoUnpublishedCourseUpload);
                }

                if (courseUpload.UploadStatus != UploadStatus.ProcessedSuccessfully &&
                    courseUpload.UploadStatus != UploadStatus.ProcessedWithErrors)
                {
                    throw new InvalidUploadStatusException(
                        courseUpload.UploadStatus,
                        UploadStatus.ProcessedSuccessfully,
                        UploadStatus.ProcessedWithErrors);
                }

                // If the world around us has changed (courses added etc.) then we might need to revalidate
                var uploadStatus = await RevalidateCourseUploadIfRequired(dispatcher, courseUpload.CourseUploadId, isNonLars);

                var (rows, _) = await dispatcher.ExecuteQuery(new GetCourseUploadRows()
                {
                    CourseUploadId = courseUpload.CourseUploadId
                });

                await dispatcher.Commit();

                return (rows, uploadStatus);
            }
        }

        public async Task<IReadOnlyCollection<CourseUploadRow>> GetCourseUploadRowsWithErrorsForProvider(Guid providerId, bool isNonLars)
        {
            using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher(System.Data.IsolationLevel.ReadCommitted))
            {
                await AcquireExclusiveCourseUploadLockForProvider(providerId, dispatcher);

                var courseUpload = await dispatcher.ExecuteQuery(new GetLatestUnpublishedCourseUploadForProvider()
                {
                    ProviderId = providerId,
                    IsNonLars = isNonLars
                });

                if (courseUpload == null)
                {
                    throw new InvalidStateException(InvalidStateReason.NoUnpublishedCourseUpload);
                }

                if (courseUpload.UploadStatus != UploadStatus.ProcessedSuccessfully &&
                    courseUpload.UploadStatus != UploadStatus.ProcessedWithErrors)
                {
                    throw new InvalidUploadStatusException(
                        courseUpload.UploadStatus,
                        UploadStatus.ProcessedSuccessfully,
                        UploadStatus.ProcessedWithErrors);
                }

                // If the world around us has changed (courses added etc.) then we might need to revalidate
                await RevalidateCourseUploadIfRequired(dispatcher, courseUpload.CourseUploadId, isNonLars);

                var (errorRows, _) = await dispatcher.ExecuteQuery(new GetCourseUploadRows()
                {
                    CourseUploadId = courseUpload.CourseUploadId,
                    WithErrorsOnly = true
                });

                await dispatcher.Commit();

                return errorRows;
            }
        }

        public IObservable<UploadStatus> GetCourseUploadStatusUpdatesForProvider(Guid providerId, bool isNonLars)
        {
            return GetCourseUploadId(isNonLars).ToObservable()
                .SelectMany(courseUploadId => GetCourseUploadStatusUpdates(courseUploadId))
                .DistinctUntilChanged()
                .TakeUntil(status => status.IsTerminal());

            async Task<Guid> GetCourseUploadId(bool isNonLars)
            {
                using var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher(System.Data.IsolationLevel.ReadCommitted);
                var courseUpload = await dispatcher.ExecuteQuery(new GetLatestUnpublishedCourseUploadForProvider() { ProviderId = providerId, IsNonLars = isNonLars });

                if (courseUpload == null)
                {
                    throw new InvalidStateException(InvalidStateReason.NoUnpublishedCourseUpload);
                }

                return courseUpload.CourseUploadId;
            }
        }

        public async Task ProcessCourseFile(Guid courseUploadId, Stream stream)
        {
            bool isNonLars = false;
            using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher(System.Data.IsolationLevel.ReadCommitted))
            {
                var uploadedCourse = await dispatcher.ExecuteQuery(new GetCourseUpload() { CourseUploadId = courseUploadId });
                isNonLars = uploadedCourse != null ?  uploadedCourse.IsNonLars : false;

                var setProcessingResult = await dispatcher.ExecuteQuery(new SetCourseUploadProcessing()
                {
                    CourseUploadId = courseUploadId,
                    ProcessingStartedOn = _clock.UtcNow
                });

                if (setProcessingResult != SetCourseUploadProcessingResult.Success)
                {
                    await DeleteBlob();

                    return;
                }

                await dispatcher.Commit();
            }
            if(isNonLars)
            {
                List<CsvNonLarsCourseRow> rows;
                using (var streamReader = new StreamReader(stream))
                using (var csvReader = CreateCsvReader(streamReader))
                {
                    rows = await csvReader.GetRecordsAsync<CsvNonLarsCourseRow>().ToListAsync();
                }
                var grouped = CsvNonLarsCourseRow.GroupRows(rows);
                var groupCourseIds = grouped.Select(g => (CourseId: Guid.NewGuid(), Rows: g)).ToArray();

                var rowInfos = new List<NonLarsCourseDataUploadRowInfo>(rows.Count);

                foreach (var row in rows)
                {
                    var courseId = groupCourseIds.Single(g => g.Rows.Contains(row)).CourseId;
                    

                    rowInfos.Add(new NonLarsCourseDataUploadRowInfo(row, rowNumber: rowInfos.Count + 2, courseId));
                }

                var rowsCollection = new NonLarsCourseDataUploadRowInfoCollection(rowInfos);

                using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher(System.Data.IsolationLevel.ReadCommitted))
                {

                    var venueUpload = await dispatcher.ExecuteQuery(new GetCourseUpload() { CourseUploadId = courseUploadId });
                    var providerId = venueUpload.ProviderId;

                    await AcquireExclusiveCourseUploadLockForProvider(providerId, dispatcher);

                    await ValidateNonLarsCourseUploadRows(dispatcher, courseUploadId, providerId, rowsCollection);

                    await dispatcher.Commit();
                }
            }
            else
            {
                // At this point `stream` should be a CSV that's already known to conform to `CsvCourseRow`'s schema.
                // We read all the rows upfront because validation needs to group rows into courses.
                // We also don't expect massive files here so reading everything into memory is ok.
                List<CsvCourseRow> rows;
                using (var streamReader = new StreamReader(stream))
                using (var csvReader = CreateCsvReader(streamReader))
                {
                    rows = await csvReader.GetRecordsAsync<CsvCourseRow>().ToListAsync();
                }

                var rowsCollection = CreateCourseDataUploadRowInfoCollection();

                using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher(System.Data.IsolationLevel.ReadCommitted))
                {
                    // If CourseName is empty, use the LearnAimRefTitle from LARS
                    var learnAimRefs = rowsCollection.Select(r => r.Data.LearnAimRef).Distinct();
                    var learningDeliveries = await dispatcher.ExecuteQuery(new GetLearningDeliveries() { LearnAimRefs = learnAimRefs });

                    foreach (var row in rowsCollection)
                    {
                        if (string.IsNullOrWhiteSpace(row.Data.CourseName))
                        {
                            row.Data.CourseName = learningDeliveries[row.Data.LearnAimRef].LearnAimRefTitle;
                        }
                    }

                    var venueUpload = await dispatcher.ExecuteQuery(new GetCourseUpload() { CourseUploadId = courseUploadId });
                    var providerId = venueUpload.ProviderId;

                    await AcquireExclusiveCourseUploadLockForProvider(providerId, dispatcher);

                    await ValidateCourseUploadRows(dispatcher, courseUploadId, providerId, rowsCollection);

                    await dispatcher.Commit();
                }

                CourseDataUploadRowInfoCollection CreateCourseDataUploadRowInfoCollection()
                {
                    // N.B. It's important we maintain ordering here; RowNumber needs to match the input

                    var grouped = CsvCourseRow.GroupRows(rows);
                    var groupCourseIds = grouped.Select(g => (CourseId: Guid.NewGuid(), Rows: g)).ToArray();

                    var rowInfos = new List<CourseDataUploadRowInfo>(rows.Count);

                    foreach (var row in rows)
                    {
                        var courseId = groupCourseIds.Single(g => g.Rows.Contains(row)).CourseId;
                        row.LearnAimRef = NormalizeLearnAimRef(row.LearnAimRef);

                        rowInfos.Add(new CourseDataUploadRowInfo(row, rowNumber: rowInfos.Count + 2, courseId));
                    }

                    return new CourseDataUploadRowInfoCollection(rowInfos);
                }
            }
            

            await DeleteBlob();

            Task DeleteBlob()
            {
                var blobName = $"{Constants.CoursesFolder}/{courseUploadId}.csv";
                return _blobContainerClient.DeleteBlobIfExistsAsync(blobName);
            }

            
        }

        public async Task<PublishResult> PublishCourseUploadForProvider(Guid providerId, UserInfo publishedBy, bool isNonLars)
        {
            using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher(System.Data.IsolationLevel.ReadCommitted))
            {
                await AcquireExclusiveCourseUploadLockForProvider(providerId, dispatcher);

                var courseUpload = await dispatcher.ExecuteQuery(new GetLatestUnpublishedCourseUploadForProvider()
                {
                    ProviderId = providerId,
                    IsNonLars = isNonLars
                });

                if (courseUpload == null)
                {
                    throw new InvalidStateException(InvalidStateReason.NoUnpublishedCourseUpload);
                }

                if (courseUpload.UploadStatus.IsUnprocessed())
                {
                    throw new InvalidUploadStatusException(
                        courseUpload.UploadStatus,
                        UploadStatus.ProcessedWithErrors,
                        UploadStatus.ProcessedSuccessfully);
                }

                if (courseUpload.UploadStatus == UploadStatus.ProcessedWithErrors)
                {
                    return PublishResult.UploadHasErrors();
                }

                var uploadStatus = await RevalidateCourseUploadIfRequired(dispatcher, courseUpload.CourseUploadId, isNonLars);

                if (uploadStatus == UploadStatus.ProcessedWithErrors)
                {
                    return PublishResult.UploadHasErrors();
                }

                var publishedOn = _clock.UtcNow;

                var publishResult = await dispatcher.ExecuteQuery(new PublishCourseUpload()
                {
                    IsNonLars = isNonLars,
                    CourseUploadId = courseUpload.CourseUploadId,
                    PublishedBy = publishedBy,
                    PublishedOn = publishedOn
                });

                await dispatcher.Commit();

                Debug.Assert(publishResult.IsT1);
                var publishedCourseRunIds = publishResult.AsT1.PublishedCourseRunIds;

                // Update the FAC index - we do this in a separate transaction in the background since it can cause timeouts
                // when done inside the PublishCourseUpload handler.
                // A mop-up job inside the Functions project ensures that any updates that fail here get captured eventually.
                await _backgroundWorkScheduler.Schedule(UpdateFindACourseIndex, state: publishedCourseRunIds);

                return PublishResult.Success(publishedCourseRunIds.Count);
            }

            static async Task UpdateFindACourseIndex(object state, IServiceProvider serviceProvider, CancellationToken cancellationToken)
            {
                const int batchSize = 200;

                var publishedCourseRunIds = (IReadOnlyCollection<Guid>)state;

                var sqlQueryDispatcherFactory = serviceProvider.GetRequiredService<ISqlQueryDispatcherFactory>();
                var clock = serviceProvider.GetRequiredService<IClock>();

                // Batch the updates to keep the transactions smaller & shorter
                foreach (var chunk in publishedCourseRunIds.Buffer(batchSize))
                {
                    using (var dispatcher = sqlQueryDispatcherFactory.CreateDispatcher(System.Data.IsolationLevel.ReadCommitted))
                    {
                        await dispatcher.ExecuteQuery(new UpdateFindACourseIndexForCourseRuns()
                        {
                            CourseRunIds = chunk,
                            Now = clock.UtcNow
                        });

                        await dispatcher.Commit();
                    }
                }
            }
        }

        public async Task<SaveCourseFileResult> SaveCourseFile(Guid providerId,bool isNonLars, Stream stream, UserInfo uploadedBy)
        {
            CheckStreamIsProcessable(stream);

            if (await FileIsEmpty(stream))
            {
                return SaveCourseFileResult.EmptyFile();
            }

            if (!await LooksLikeCsv(stream))
            {
                return SaveCourseFileResult.InvalidFile();
            }
            
            if(isNonLars)
            {
                var (fileMatchesSchemaResult, missingHeaders) = await FileMatchesSchema<CsvNonLarsCourseRow>(stream);
                if (fileMatchesSchemaResult == FileMatchesSchemaResult.InvalidHeader)
                {
                    return SaveCourseFileResult.InvalidHeader(missingHeaders);
                }
            }
            else
            {
                var (fileMatchesSchemaResult, missingHeaders) = await FileMatchesSchema<CsvCourseRow>(stream);
                if (fileMatchesSchemaResult == FileMatchesSchemaResult.InvalidHeader)
                {
                    return SaveCourseFileResult.InvalidHeader(missingHeaders);
                }
                var (missingLars, invalidLars, expiredLars) = await ValidateLearnAimRefs(stream);

                if (missingLars.Length > 0 || invalidLars.Length > 0 || expiredLars.Length > 0)
                {
                    return SaveCourseFileResult.InvalidLars(missingLars, invalidLars, expiredLars);
                }
            }
            
            var courseUploadId = Guid.NewGuid();

            using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher(System.Data.IsolationLevel.ReadCommitted))
            {
                await AcquireExclusiveCourseUploadLockForProvider(providerId, dispatcher);

                // Check there isn't an existing unprocessed upload for this provider

                var existingUpload = await dispatcher.ExecuteQuery(new GetLatestUnpublishedCourseUploadForProvider()
                {
                    ProviderId = providerId,
                    IsNonLars = isNonLars
                });

                if (existingUpload != null && existingUpload.UploadStatus.IsUnprocessed())
                {
                    return SaveCourseFileResult.ExistingFileInFlight();
                }

                // Abandon any existing un-published upload (there will be one at most)
                if (existingUpload != null)
                {
                    await dispatcher.ExecuteQuery(new SetCourseUploadAbandoned()
                    {
                        CourseUploadId = existingUpload.CourseUploadId,
                        AbandonedOn = _clock.UtcNow
                    });
                }

                await dispatcher.ExecuteQuery(new CreateCourseUpload()
                {
                    CourseUploadId = courseUploadId,
                    CreatedBy = uploadedBy,
                    CreatedOn = _clock.UtcNow,
                    ProviderId = providerId,
                    IsNonLars = isNonLars
                });

                await dispatcher.Transaction.CommitAsync();
            }

            await UploadToBlobStorage();

            return SaveCourseFileResult.Success(courseUploadId, UploadStatus.Created);

            async Task UploadToBlobStorage()
            {
                if (!_containerIsKnownToExist)
                {
                    await _blobContainerClient.CreateIfNotExistsAsync();
                    _containerIsKnownToExist = true;
                }

                var blobName = $"{Constants.CoursesFolder}/{courseUploadId}.csv";
                await _blobContainerClient.UploadBlobAsync(blobName, stream);
            }
        }

        public async Task<UploadStatus> UpdateCourseUploadRowForProvider(Guid providerId, int rowNumber, bool isNonLars, CourseUploadRowUpdate update)
        {
            using var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher(System.Data.IsolationLevel.ReadCommitted);

            await AcquireExclusiveCourseUploadLockForProvider(providerId, dispatcher);

            var courseUpload = await dispatcher.ExecuteQuery(new GetLatestUnpublishedCourseUploadForProvider()
            {
                ProviderId = providerId,
                IsNonLars = isNonLars
            });

            if (courseUpload == null)
            {
                throw new InvalidStateException(InvalidStateReason.NoUnpublishedCourseUpload);
            }

            if (courseUpload.UploadStatus != UploadStatus.ProcessedWithErrors)
            {
                throw new InvalidUploadStatusException(courseUpload.UploadStatus, UploadStatus.ProcessedWithErrors);
            }

            var row = await dispatcher.ExecuteQuery(new GetCourseUploadRowDetail()
            {
                CourseUploadId = courseUpload.CourseUploadId,
                RowNumber = rowNumber
            });

            if (row == null)
            {
                throw new ResourceDoesNotExistException(ResourceType.CourseUploadRow, rowNumber);
            }

            var allRegions = await _regionCache.GetAllRegions();

            Venue venue = update.VenueId.HasValue ?
                (await dispatcher.ExecuteQuery(new GetVenue() { VenueId = update.VenueId.Value })) :
                null;
            Debug.Assert(!update.VenueId.HasValue || venue != null);
            if(isNonLars)
            {
                var updatedRows = new NonLarsCourseDataUploadRowInfoCollection(
               new NonLarsCourseDataUploadRowInfo(
                   new CsvNonLarsCourseRow()
                   {
                       CourseType = ParsedCsvNonLarsCourseRow.MapCourseType(update.CourseType),
                       Sector = RemoveASCII(update.Sector),
                       AwardingBody = RemoveASCII(update.AwardingBody),
                       EducationLevel = ParsedCsvNonLarsCourseRow.MapEducationLevel(update.EducationLevel),
                       AttendancePattern = ParsedCsvNonLarsCourseRow.MapAttendancePattern(update.AttendancePattern),
                       Cost = ParsedCsvNonLarsCourseRow.MapCost(update.Cost),
                       CostDescription = RemoveASCII(update.CostDescription),
                       CourseName = update.CourseName,
                       CourseWebPage = update.CourseWebPage,
                       DeliveryMode = ParsedCsvNonLarsCourseRow.MapDeliveryMode(update.DeliveryMode),
                       Duration = ParsedCsvNonLarsCourseRow.MapDuration(update.Duration),
                       DurationUnit = ParsedCsvNonLarsCourseRow.MapDurationUnit(update.DurationUnit),
                       EntryRequirements = RemoveASCII(row.EntryRequirements),
                       FlexibleStartDate = ParsedCsvNonLarsCourseRow.MapFlexibleStartDate(update.FlexibleStartDate),
                       HowYouWillBeAssessed = RemoveASCII(row.HowYouWillBeAssessed),
                       HowYouWillLearn = RemoveASCII(row.HowYouWillLearn),
                       NationalDelivery = ParsedCsvNonLarsCourseRow.MapNationalDelivery(update.NationalDelivery),
                       ProviderCourseRef = update.ProviderCourseRef,
                       ProviderVenueRef = venue?.ProviderVenueRef,
                       StartDate = ParsedCsvNonLarsCourseRow.MapStartDate(update.StartDate),
                       StudyMode = ParsedCsvNonLarsCourseRow.MapStudyMode(update.StudyMode),
                       SubRegions = ParsedCsvNonLarsCourseRow.MapSubRegions(update.SubRegionIds, allRegions),
                       VenueName = venue?.VenueName,
                       WhatYouWillLearn = RemoveASCII(row.WhatYouWillLearn),
                       WhatYouWillNeedToBring = RemoveASCII(row.WhatYouWillNeedToBring),
                       WhereNext = RemoveASCII(row.WhereNext),
                       WhoThisCourseIsFor = RemoveASCII(row.WhoThisCourseIsFor)
                   },
                   row.RowNumber,
                   row.CourseId,
                   venue?.VenueId));

                await ValidateNonLarsCourseUploadRows(dispatcher, courseUpload.CourseUploadId, courseUpload.ProviderId, updatedRows);

            }
            else
            {
                var updatedRows = new CourseDataUploadRowInfoCollection(
               new CourseDataUploadRowInfo(
                   new CsvCourseRow()
                   {
                       AttendancePattern = ParsedCsvCourseRow.MapAttendancePattern(update.AttendancePattern),
                       Cost = ParsedCsvCourseRow.MapCost(update.Cost),
                       CostDescription = RemoveASCII(update.CostDescription),
                       CourseName = update.CourseName,
                       CourseWebPage = update.CourseWebPage,
                       DeliveryMode = ParsedCsvCourseRow.MapDeliveryMode(update.DeliveryMode),
                       Duration = ParsedCsvCourseRow.MapDuration(update.Duration),
                       DurationUnit = ParsedCsvCourseRow.MapDurationUnit(update.DurationUnit),
                       EntryRequirements = RemoveASCII(row.EntryRequirements),
                       FlexibleStartDate = ParsedCsvCourseRow.MapFlexibleStartDate(update.FlexibleStartDate),
                       HowYouWillBeAssessed = RemoveASCII(row.HowYouWillBeAssessed),
                       HowYouWillLearn = RemoveASCII(row.HowYouWillLearn),
                       LearnAimRef = row.LearnAimRef,
                       NationalDelivery = ParsedCsvCourseRow.MapNationalDelivery(update.NationalDelivery),
                       ProviderCourseRef = update.ProviderCourseRef,
                       ProviderVenueRef = venue?.ProviderVenueRef,
                       StartDate = ParsedCsvCourseRow.MapStartDate(update.StartDate),
                       StudyMode = ParsedCsvCourseRow.MapStudyMode(update.StudyMode),
                       SubRegions = ParsedCsvCourseRow.MapSubRegions(update.SubRegionIds, allRegions),
                       VenueName = venue?.VenueName,
                       WhatYouWillLearn = RemoveASCII(row.WhatYouWillLearn),
                       WhatYouWillNeedToBring = RemoveASCII(row.WhatYouWillNeedToBring),
                       WhereNext = RemoveASCII(row.WhereNext),
                       WhoThisCourseIsFor = RemoveASCII(row.WhoThisCourseIsFor)
                   },
                   row.RowNumber,
                   row.CourseId,
                   venue?.VenueId));

                await ValidateCourseUploadRows(dispatcher, courseUpload.CourseUploadId, courseUpload.ProviderId, updatedRows);
            }

            // Other rows not covered by this group may require revalidation;
            // ensure revalidation is done if required so that `uploadStatus` is accurate
            var uploadStatus = await RevalidateCourseUploadIfRequired(dispatcher, courseUpload.CourseUploadId, isNonLars);

            await dispatcher.Commit();

            return uploadStatus;
        }

        public async Task<UploadStatus> UpdateCourseUploadRowGroupForProvider(Guid providerId, Guid courseId, CourseUploadRowGroupUpdate update, bool isNonLars)
        {
            using var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher(System.Data.IsolationLevel.ReadCommitted);

            await AcquireExclusiveCourseUploadLockForProvider(providerId, dispatcher);

            var courseUpload = await dispatcher.ExecuteQuery(new GetLatestUnpublishedCourseUploadForProvider()
            {
                ProviderId = providerId,
                IsNonLars = isNonLars
            });

            if (courseUpload == null)
            {
                throw new InvalidStateException(InvalidStateReason.NoUnpublishedCourseUpload);
            }

            if (courseUpload.UploadStatus != UploadStatus.ProcessedWithErrors)
            {
                throw new InvalidUploadStatusException(courseUpload.UploadStatus, UploadStatus.ProcessedWithErrors);
            }

            var rows = await dispatcher.ExecuteQuery(new GetCourseUploadRowsByCourseId()
            {
                CourseUploadId = courseUpload.CourseUploadId,
                CourseId = courseId
            });

            if (rows.Count == 0)
            {
                throw new ResourceDoesNotExistException(ResourceType.CourseUploadRowGroup, courseId);
            }
            if(isNonLars)
            {
                var updatedRows = new NonLarsCourseDataUploadRowInfoCollection(
               rows.Select(r =>
                   new NonLarsCourseDataUploadRowInfo(
                       new CsvNonLarsCourseRow()
                       {
                           AttendancePattern = r.AttendancePattern,
                           Cost = r.Cost,
                           CostDescription = r.CostDescription,
                           CourseName = r.CourseName,
                           CourseWebPage = r.CourseWebPage,
                           DeliveryMode = r.DeliveryMode,
                           Duration = r.Duration,
                           DurationUnit = r.DurationUnit,
                           EntryRequirements = RemoveASCII(update.EntryRequirements),
                           FlexibleStartDate = r.FlexibleStartDate,
                           HowYouWillBeAssessed = RemoveASCII(update.HowYouWillBeAssessed),
                           HowYouWillLearn = RemoveASCII(update.HowYouWillLearn),
                           CourseType = r.CourseType,
                           NationalDelivery = r.NationalDelivery,
                           ProviderCourseRef = r.ProviderCourseRef,
                           ProviderVenueRef = r.ProviderVenueRef,
                           StartDate = r.StartDate,
                           StudyMode = r.StudyMode,
                           SubRegions = r.SubRegions,
                           VenueName = r.VenueName,
                           WhatYouWillLearn = RemoveASCII(update.WhatYouWillLearn),
                           WhatYouWillNeedToBring = RemoveASCII(update.WhatYouWillNeedToBring),
                           WhereNext = RemoveASCII(update.WhereNext),
                           WhoThisCourseIsFor = RemoveASCII(update.WhoThisCourseIsFor)
                       },
                       r.RowNumber,
                       courseId)));

                await ValidateNonLarsCourseUploadRows(dispatcher, courseUpload.CourseUploadId, courseUpload.ProviderId, updatedRows);
            }
            else
            {
                var updatedRows = new CourseDataUploadRowInfoCollection(
                rows.Select(r =>
                    new CourseDataUploadRowInfo(
                        new CsvCourseRow()
                        {
                            AttendancePattern = r.AttendancePattern,
                            Cost = r.Cost,
                            CostDescription = r.CostDescription,
                            CourseName = r.CourseName,
                            CourseWebPage = r.CourseWebPage,
                            DeliveryMode = r.DeliveryMode,
                            Duration = r.Duration,
                            DurationUnit = r.DurationUnit,
                            EntryRequirements = RemoveASCII(update.EntryRequirements),
                            FlexibleStartDate = r.FlexibleStartDate,
                            HowYouWillBeAssessed = RemoveASCII(update.HowYouWillBeAssessed),
                            HowYouWillLearn = RemoveASCII(update.HowYouWillLearn),
                            LearnAimRef = r.LearnAimRef,
                            NationalDelivery = r.NationalDelivery,
                            ProviderCourseRef = r.ProviderCourseRef,
                            ProviderVenueRef = r.ProviderVenueRef,
                            StartDate = r.StartDate,
                            StudyMode = r.StudyMode,
                            SubRegions = r.SubRegions,
                            VenueName = r.VenueName,
                            WhatYouWillLearn = RemoveASCII(update.WhatYouWillLearn),
                            WhatYouWillNeedToBring = RemoveASCII(update.WhatYouWillNeedToBring),
                            WhereNext = RemoveASCII(update.WhereNext),
                            WhoThisCourseIsFor = RemoveASCII(update.WhoThisCourseIsFor)
                        },
                        r.RowNumber,
                        courseId)));

                await ValidateCourseUploadRows(dispatcher, courseUpload.CourseUploadId, courseUpload.ProviderId, updatedRows);


            }

            // Other rows not covered by this group may require revalidation;
            // ensure revalidation is done if required so that `uploadStatus` is accurate
            var uploadStatus = await RevalidateCourseUploadIfRequired(dispatcher, courseUpload.CourseUploadId, isNonLars);
            await dispatcher.Commit();

            return uploadStatus;
        }

        protected async Task<UploadStatus> GetCourseUploadStatus(Guid courseUploadId)
        {
            using var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher(System.Data.IsolationLevel.ReadCommitted);
            var courseUpload = await dispatcher.ExecuteQuery(new GetCourseUpload() { CourseUploadId = courseUploadId });

            if (courseUpload == null)
            {
                throw new ArgumentException("Specified course upload does not exist.", nameof(courseUploadId));
            }

            return courseUpload.UploadStatus;
        }

        // virtual for testing
        protected virtual IObservable<UploadStatus> GetCourseUploadStatusUpdates(Guid courseUploadId) =>
            Observable.Interval(_statusUpdatesPollInterval)
                .SelectMany(_ => Observable.FromAsync(() => GetCourseUploadStatus(courseUploadId)));

        // internal for testing
        internal Venue FindVenue(Guid? venueIdHint, string venueName,string providerVenueRef, IReadOnlyCollection<Venue> providerVenues)
        {
            if (venueIdHint.HasValue)
            {
                return providerVenues.Single(v => v.VenueId == venueIdHint);
            }

            if (!string.IsNullOrEmpty(providerVenueRef))
            {
                // N.B. Using `Count()` here instead of `Single()` to protect against bad data where we have duplicates

                var matchedVenues = providerVenues
                    .Where(v => RefMatches(providerVenueRef, v))
                    .ToArray();

                if (matchedVenues.Length != 1)
                {
                    return null;
                }

                var venue = matchedVenues[0];

                // If VenueName was provided too then it must match
                if (!string.IsNullOrEmpty(venueName) && !NameMatches(venueName, venue))
                {
                    return null;
                }

                return venue;
            }

            if (!string.IsNullOrEmpty(venueName))
            {
                // N.B. Using `Count()` here instead of `Single()` to protect against bad data where we have duplicates

                var matchedVenues = providerVenues
                    .Where(v => NameMatches(venueName, v))
                    .ToArray();

                if (matchedVenues.Length != 1)
                {
                    return null;
                }

                return matchedVenues[0];
            }

            return null;

            static bool NameMatches(string name, Venue venue) => name.Equals(venue.VenueName, StringComparison.OrdinalIgnoreCase);

            static bool RefMatches(string providerVenueRef, Venue venue) => providerVenueRef.Equals(venue.ProviderVenueRef, StringComparison.OrdinalIgnoreCase);
        }

        // internal for testing
        internal async Task<IReadOnlyCollection<CourseUploadRow>> GetCourseUploadRowsRequiringRevalidation(
            ISqlQueryDispatcher sqlQueryDispatcher,
            CourseUpload courseUpload)
        {
            if (courseUpload.UploadStatus != UploadStatus.ProcessedWithErrors &&
                courseUpload.UploadStatus != UploadStatus.ProcessedSuccessfully)
            {
                throw new InvalidOperationException($"Course upload at status {courseUpload.UploadStatus} cannot be revalidated.");
            }

            // We need to revalidate any rows that are linked to venues where either
            // the linked venue has been amended/deleted (it may not match now)
            // or where a new venue has been added (there may be a match where there wasn't before).
            //
            // Note this is a different approach to venues (where we have to revalidate the entire file);
            // for courses we want to minimize the amount of data we're shuttling back and forth from the DB.

            return await sqlQueryDispatcher.ExecuteQuery(
                new GetCourseUploadRowsToRevalidate() { CourseUploadId = courseUpload.CourseUploadId });
        }

        internal async Task<UploadStatus> RevalidateCourseUploadIfRequired(
            ISqlQueryDispatcher sqlQueryDispatcher,
            Guid courseUploadId, bool isNonLars)
        {
            var courseUpload = await sqlQueryDispatcher.ExecuteQuery(new GetCourseUpload() { CourseUploadId = courseUploadId });

            if (courseUpload == null)
            {
                throw new ArgumentException("Course upload does not exist.", nameof(courseUploadId));
            }

            var toBeRevalidated = await GetCourseUploadRowsRequiringRevalidation(sqlQueryDispatcher, courseUpload);

            if (toBeRevalidated.Count == 0)
            {
                return courseUpload.UploadStatus;
            }
            if(isNonLars)
            {
                var nonLarsRows = new NonLarsCourseDataUploadRowInfoCollection(
                    toBeRevalidated.Select(r => new NonLarsCourseDataUploadRowInfo(CsvNonLarsCourseRow.FromModel(r), r.RowNumber, r.CourseId)));

                var (nonLarsStatus, _) = await ValidateNonLarsCourseUploadRows(sqlQueryDispatcher, courseUploadId, courseUpload.ProviderId, nonLarsRows);

                return nonLarsStatus;
            }
            else
            {
                var rowsCollection = new CourseDataUploadRowInfoCollection(
                    toBeRevalidated.Select(r => new CourseDataUploadRowInfo(CsvCourseRow.FromModel(r), r.RowNumber, r.CourseId)));

                var (uploadStatus, _) = await ValidateCourseUploadRows(sqlQueryDispatcher, courseUploadId, courseUpload.ProviderId, rowsCollection);

                return uploadStatus;
            }

        }

        // internal for testing
        internal async Task<(UploadStatus uploadStatus, IReadOnlyCollection<CourseUploadRow> Rows)> ValidateCourseUploadRows(
            ISqlQueryDispatcher sqlQueryDispatcher,
            Guid courseUploadId,
            Guid providerId,
            CourseDataUploadRowInfoCollection rows)
        {
            var allRegions = await _regionCache.GetAllRegions();

            var providerVenues = await sqlQueryDispatcher.ExecuteQuery(new GetVenuesByProvider() { ProviderId = providerId });            

            var rowsAreValid = true;

            var upsertRecords = new List<UpsertCourseUploadRowsRecord>();

            foreach (var row in rows)
            {
                var rowNumber = row.RowNumber;
                var courseRunId = Guid.NewGuid();

                var parsedRow = ParsedCsvCourseRow.FromCsvCourseRow(row.Data, allRegions);

                var matchedVenue = FindVenue(row.VenueIdHint, row.Data.VenueName, row.Data.ProviderVenueRef, providerVenues);

                var courseType = await _courseTypeService.GetCourseType(parsedRow.LearnAimRef, providerId);

                var validator = new CourseUploadRowValidator(_clock, matchedVenue?.VenueId);

                var rowValidationResult = validator.Validate(parsedRow);
                var errors = rowValidationResult.Errors.Select(e => e.ErrorCode).ToArray();
                var rowIsValid = rowValidationResult.IsValid;
                rowsAreValid &= rowIsValid;

                upsertRecords.Add(new UpsertCourseUploadRowsRecord()
                {
                    RowNumber = rowNumber,
                    IsValid = rowIsValid,
                    Errors = errors,
                    CourseId = row.CourseId,
                    CourseRunId = courseRunId,
                    LearnAimRef = parsedRow.LearnAimRef,
                    WhoThisCourseIsFor = RemoveASCII(parsedRow.WhoThisCourseIsFor),
                    EntryRequirements = RemoveASCII(parsedRow.EntryRequirements),
                    WhatYouWillLearn = RemoveASCII(parsedRow.WhatYouWillLearn),
                    HowYouWillLearn = RemoveASCII(parsedRow.HowYouWillLearn),
                    WhatYouWillNeedToBring = RemoveASCII(parsedRow.WhatYouWillNeedToBring),
                    HowYouWillBeAssessed = RemoveASCII(parsedRow.HowYouWillBeAssessed),
                    WhereNext = RemoveASCII(parsedRow.WhereNext),
                    CostDescription = RemoveASCII(parsedRow.CostDescription),
                    CourseName = parsedRow.CourseName,
                    ProviderCourseRef = parsedRow.ProviderCourseRef,
                    DeliveryMode = ParsedCsvCourseRow.MapDeliveryMode(parsedRow.ResolvedDeliveryMode) ?? parsedRow.DeliveryMode,
                    StartDate = ParsedCsvCourseRow.MapStartDate(parsedRow.ResolvedStartDate) ?? parsedRow.StartDate,
                    FlexibleStartDate = ParsedCsvCourseRow.MapFlexibleStartDate(parsedRow.ResolvedFlexibleStartDate) ?? parsedRow.FlexibleStartDate,
                    VenueName = matchedVenue?.VenueName ?? parsedRow.VenueName,
                    ProviderVenueRef = matchedVenue?.ProviderVenueRef ?? parsedRow.ProviderVenueRef,
                    NationalDelivery = ParsedCsvCourseRow.MapNationalDelivery(parsedRow.ResolvedNationalDelivery) ?? parsedRow.NationalDelivery,
                    SubRegions = parsedRow.SubRegions,
                    CourseWebpage = parsedRow.CourseWebPage,
                    Cost = ParsedCsvCourseRow.MapCost(parsedRow.ResolvedCost) ?? parsedRow.Cost,
                    Duration = ParsedCsvCourseRow.MapDuration(parsedRow.ResolvedDuration) ?? parsedRow.Duration,
                    DurationUnit = ParsedCsvCourseRow.MapDurationUnit(parsedRow.ResolvedDurationUnit) ?? parsedRow.DurationUnit,
                    StudyMode = ParsedCsvCourseRow.MapStudyMode(parsedRow.ResolvedStudyMode) ?? parsedRow.StudyMode,
                    VenueId = matchedVenue?.VenueId,
                    AttendancePattern = parsedRow.AttendancePattern,
                    ResolvedDeliveryMode = parsedRow.ResolvedDeliveryMode,
                    ResolvedStartDate = parsedRow.ResolvedStartDate,
                    ResolvedFlexibleStartDate = parsedRow.ResolvedFlexibleStartDate,
                    ResolvedNationalDelivery = parsedRow.ResolvedNationalDelivery,
                    ResolvedCost = parsedRow.ResolvedCost,
                    ResolvedDuration = parsedRow.ResolvedDuration,
                    ResolvedDurationUnit = parsedRow.ResolvedDurationUnit,
                    ResolvedStudyMode = parsedRow.ResolvedStudyMode,
                    ResolvedAttendancePattern = parsedRow.ResolvedAttendancePattern,
                    ResolvedSubRegions = parsedRow.ResolvedSubRegions?.Select(sr => sr.Id)?.ToArray(),
                    CourseType = ParsedCsvCourseRow.MapCourseType(courseType),
                    ResolvedCourseType = courseType
                });
            }

            var updatedRows = await sqlQueryDispatcher.ExecuteQuery(new UpsertCourseUploadRows()
            {
                CourseUploadId = courseUploadId,
                ValidatedOn = _clock.UtcNow,
                Records = upsertRecords
            });

            var uploadStatus = await RefreshCourseUploadValidationStatus(courseUploadId, sqlQueryDispatcher);

            return (uploadStatus, updatedRows);
        }
        internal async Task<(UploadStatus uploadStatus, IReadOnlyCollection<CourseUploadRow> Rows)> ValidateNonLarsCourseUploadRows(
          ISqlQueryDispatcher sqlQueryDispatcher,
          Guid courseUploadId,
          Guid providerId,
          NonLarsCourseDataUploadRowInfoCollection rows)
        {
            var allRegions = await _regionCache.GetAllRegions();

            var providerVenues = await sqlQueryDispatcher.ExecuteQuery(new GetVenuesByProvider() { ProviderId = providerId });

            var sectors = (await sqlQueryDispatcher.ExecuteQuery(new GetSectors())).ToList();

            var rowsAreValid = true;

            var upsertRecords = new List<UpsertCourseUploadRowsRecord>();

            foreach (var row in rows)
            {
                var rowNumber = row.RowNumber;
                var courseRunId = Guid.NewGuid();

                var parsedRow = ParsedCsvNonLarsCourseRow.FromCsvCourseRow(row.Data, allRegions, sectors);

                var matchedVenue = FindVenue(row.VenueIdHint, row.Data.VenueName, row.Data.ProviderVenueRef, providerVenues);

                var validator = new NonLarsCourseUploadRowValidator(_clock, matchedVenue?.VenueId);

                var rowValidationResult = validator.Validate(parsedRow);
                var errors = rowValidationResult.Errors.Select(e => e.ErrorCode).ToArray();
                var rowIsValid = rowValidationResult.IsValid;
                rowsAreValid &= rowIsValid;

                upsertRecords.Add(new UpsertCourseUploadRowsRecord()
                {
                    RowNumber = rowNumber,
                    IsValid = rowIsValid,
                    Errors = errors,
                    CourseId = row.CourseId,
                    CourseRunId = courseRunId,
                    WhoThisCourseIsFor = RemoveASCII(parsedRow.WhoThisCourseIsFor),
                    EntryRequirements = RemoveASCII(parsedRow.EntryRequirements),
                    WhatYouWillLearn = RemoveASCII(parsedRow.WhatYouWillLearn),
                    HowYouWillLearn = RemoveASCII(parsedRow.HowYouWillLearn),
                    WhatYouWillNeedToBring = RemoveASCII(parsedRow.WhatYouWillNeedToBring),
                    HowYouWillBeAssessed = RemoveASCII(parsedRow.HowYouWillBeAssessed),
                    WhereNext = RemoveASCII(parsedRow.WhereNext),
                    CostDescription = RemoveASCII(parsedRow.CostDescription),
                    CourseName = parsedRow.CourseName,
                    ProviderCourseRef = parsedRow.ProviderCourseRef,
                    DeliveryMode = ParsedCsvNonLarsCourseRow.MapDeliveryMode(parsedRow.ResolvedDeliveryMode) ?? parsedRow.DeliveryMode,
                    StartDate = ParsedCsvNonLarsCourseRow.MapStartDate(parsedRow.ResolvedStartDate) ?? parsedRow.StartDate,
                    FlexibleStartDate = ParsedCsvNonLarsCourseRow.MapFlexibleStartDate(parsedRow.ResolvedFlexibleStartDate) ?? parsedRow.FlexibleStartDate,
                    VenueName = matchedVenue?.VenueName ?? parsedRow.VenueName,
                    ProviderVenueRef = matchedVenue?.ProviderVenueRef ?? parsedRow.ProviderVenueRef,
                    NationalDelivery = ParsedCsvNonLarsCourseRow.MapNationalDelivery(parsedRow.ResolvedNationalDelivery) ?? parsedRow.NationalDelivery,
                    SubRegions = parsedRow.SubRegions,
                    CourseWebpage = parsedRow.CourseWebPage,
                    Cost = ParsedCsvNonLarsCourseRow.MapCost(parsedRow.ResolvedCost) ?? parsedRow.Cost,
                    Duration = ParsedCsvNonLarsCourseRow.MapDuration(parsedRow.ResolvedDuration) ?? parsedRow.Duration,
                    DurationUnit = ParsedCsvNonLarsCourseRow.MapDurationUnit(parsedRow.ResolvedDurationUnit) ?? parsedRow.DurationUnit,
                    StudyMode = ParsedCsvNonLarsCourseRow.MapStudyMode(parsedRow.ResolvedStudyMode) ?? parsedRow.StudyMode,
                    VenueId = matchedVenue?.VenueId,
                    AttendancePattern = parsedRow.AttendancePattern,
                    ResolvedDeliveryMode = parsedRow.ResolvedDeliveryMode,
                    ResolvedStartDate = parsedRow.ResolvedStartDate,
                    ResolvedFlexibleStartDate = parsedRow.ResolvedFlexibleStartDate,
                    ResolvedNationalDelivery = parsedRow.ResolvedNationalDelivery,
                    ResolvedCost = parsedRow.ResolvedCost,
                    ResolvedDuration = parsedRow.ResolvedDuration,
                    ResolvedDurationUnit = parsedRow.ResolvedDurationUnit,
                    ResolvedStudyMode = parsedRow.ResolvedStudyMode,
                    ResolvedAttendancePattern = parsedRow.ResolvedAttendancePattern,
                    ResolvedSubRegions = parsedRow.ResolvedSubRegions?.Select(sr => sr.Id)?.ToArray(),
                    CourseType = ParsedCsvNonLarsCourseRow.MapCourseType(parsedRow.ResolvedCourseType) ?? parsedRow.CourseType,
                    ResolvedCourseType = parsedRow.ResolvedCourseType,
                    Sector = parsedRow.Sector,
                    ResolvedSector = parsedRow.ResolvedSector,
                    EducationLevel = ParsedCsvNonLarsCourseRow.MapEducationLevel(parsedRow.ResolvedEducationLevel) ?? parsedRow.EducationLevel,
                    ResolvedEducationLevel = parsedRow.ResolvedEducationLevel,
                    AwardingBody = parsedRow.AwardingBody,
                });
            }

            var updatedRows = await sqlQueryDispatcher.ExecuteQuery(new UpsertCourseUploadRows()
            {
                CourseUploadId = courseUploadId,
                ValidatedOn = _clock.UtcNow,
                Records = upsertRecords
            });

            var uploadStatus = await RefreshCourseUploadValidationStatus(courseUploadId, sqlQueryDispatcher);

            return (uploadStatus, updatedRows);
        }
        internal async Task<(int[] Missing, (string LearnAimRef, int RowNumber)[] Invalid, (string LearnAimRef, int RowNumber)[] Expired)> ValidateLearnAimRefs(Stream stream)
        {
            CheckStreamIsProcessable(stream);

            try
            {
                var missing = new List<int>();
                var invalid = new List<(string LearnAimRef, int RowNumber)>();
                var expired = new List<(string LearnAimRef, int RowNumber)>();

                using (var streamReader = new StreamReader(stream, leaveOpen: true))
                using (var csvReader = CreateCsvReader(streamReader))
                using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher(System.Data.IsolationLevel.ReadCommitted))
                {
                    await csvReader.ReadAsync();
                    csvReader.ReadHeader();

                    var rowLearnAimRefs = csvReader.GetRecords<CsvCourseRow>()
                        .Select(row => NormalizeLearnAimRef(row.LearnAimRef))
                        .ToList();

                    var validLearningDeliveries = await dispatcher.ExecuteQuery(
                        new GetLearningDeliveries() { LearnAimRefs = rowLearnAimRefs.Distinct() });

                    int rowNumber = 2;

                    foreach (var learnAimRef in rowLearnAimRefs)
                    {
                        if (string.IsNullOrWhiteSpace(learnAimRef))
                        {
                            missing.Add(rowNumber);
                        }
                        else if (!validLearningDeliveries.TryGetValue(learnAimRef, out var learningDelivery))
                        {
                            invalid.Add((learnAimRef, rowNumber));
                        }
                        else if (await IsExpiredInValidityCheckAsync(learnAimRef))
                        {
                            expired.Add((learnAimRef, rowNumber));
                        }

                        rowNumber++;
                    }
                }

                return (missing.ToArray(), invalid.ToArray(), expired.ToArray());
            }
            finally
            {
                stream.Seek(0L, SeekOrigin.Begin);
            }
        }

        private async Task<bool> IsExpiredInValidityCheckAsync(string learnAimRef)
        {
            bool expired = true;
            using var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher(System.Data.IsolationLevel.ReadCommitted);
            var lastNewStartDates = await dispatcher.ExecuteQuery(new GetValidityLastNewStartDate() { LearnAimRef = learnAimRef });
            
            if (lastNewStartDates.Count() > 0)
            {
                if (lastNewStartDates.Contains(string.Empty))
                    expired = false;
                else
                {
                    List<DateTime> dates = new List<DateTime>();
                    foreach(var  date in lastNewStartDates)
                    {
                        DateTime result = DateTime.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                        dates.Add(result);
                    }
                    DateTime latestdate = dates.OrderByDescending(x => x).First();
                    if (latestdate>DateTime.Now)
                        expired = false;
                }
            }
            return expired;
        }

        private static string NormalizeLearnAimRef(string value)
        {
            var trimmed = value?.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(trimmed))
            {
                return trimmed;
            }

            return trimmed.PadLeft(8, '0');
        }

        private async Task AcquireExclusiveCourseUploadLockForProvider(Guid providerId, ISqlQueryDispatcher sqlQueryDispatcher)
        {
            var lockName = $"DM_Courses:{providerId}";
            const int timeoutMilliseconds = 3000;

            var acquired = await sqlQueryDispatcher.ExecuteQuery(new GetExclusiveLock()
            {
                Name = lockName,
                TimeoutMilliseconds = timeoutMilliseconds
            });

            if (!acquired)
            {
                throw new Exception($"Failed to acquire exclusive course upload lock for provider {providerId}.");
            }
        }

        private async Task<UploadStatus> RefreshCourseUploadValidationStatus(Guid courseUploadId, ISqlQueryDispatcher sqlQueryDispatcher)
        {
            var uploadIsValid = (await sqlQueryDispatcher.ExecuteQuery(new GetCourseUploadInvalidRowCount() { CourseUploadId = courseUploadId })) == 0;

            await sqlQueryDispatcher.ExecuteQuery(new SetCourseUploadProcessed()
            {
                CourseUploadId = courseUploadId,
                ProcessingCompletedOn = _clock.UtcNow,
                IsValid = uploadIsValid
            });

            return uploadIsValid ? UploadStatus.ProcessedSuccessfully : UploadStatus.ProcessedWithErrors;
        }

        // internal for testing
        internal class CourseUploadRowValidator : AbstractValidator<ParsedCsvCourseRow>
        {
            public CourseUploadRowValidator(
                IClock clock,
                Guid? matchedVenueId)
            {
                RuleFor(c => c.WhoThisCourseIsFor).WhoThisCourseIsFor();
                RuleFor(c => c.EntryRequirements).EntryRequirements();
                RuleFor(c => c.WhatYouWillLearn).WhatYouWillLearn();
                RuleFor(c => c.HowYouWillLearn).HowYouWillLearn();
                RuleFor(c => c.WhatYouWillNeedToBring).WhatYouWillNeedToBring();
                RuleFor(c => c.HowYouWillBeAssessed).HowYouWillBeAssessed();
                RuleFor(c => c.WhereNext).WhereNext();
                RuleFor(c => c.CourseName).CourseName();
                RuleFor(c => c.ProviderCourseRef).ProviderCourseRef();
                RuleFor(c => c.ResolvedDeliveryMode).DeliveryMode();
                RuleFor(c => c.ResolvedStartDate)
                    .Transform(d => d.HasValue ? new DateInput(d.Value) : null)
                    .StartDate(clock.UtcNow, c => c.ResolvedFlexibleStartDate);
                RuleFor(c => c.ResolvedFlexibleStartDate).FlexibleStartDate();
                RuleFor(c => c.VenueName).VenueName(c => c.ResolvedDeliveryMode, c => c.ProviderVenueRef, matchedVenueId);
                RuleFor(c => c.ProviderVenueRef).ProviderVenueRef(c => c.ResolvedDeliveryMode, c => c.VenueName, matchedVenueId);
                RuleFor(c => c.ResolvedNationalDelivery).NationalDelivery(c => c.ResolvedDeliveryMode);
                RuleFor(c => c.ResolvedSubRegions).SubRegions(
                    subRegionsWereSpecified: c => !string.IsNullOrEmpty(c.SubRegions),
                    c => c.ResolvedDeliveryMode,
                    c => c.ResolvedNationalDelivery);
                RuleFor(c => c.CourseWebPage).CourseWebPage();
                RuleFor(c => c.ResolvedCost).Cost(costWasSpecified: c => !string.IsNullOrEmpty(c.Cost), c => c.CostDescription);
                RuleFor(c => c.CostDescription).CostDescription();
                RuleFor(c => c.ResolvedDuration).Duration();
                RuleFor(c => c.ResolvedDurationUnit).DurationUnit();
                RuleFor(c => c.ResolvedStudyMode).StudyMode(
                    studyModeWasSpecified: t => !string.IsNullOrEmpty(t.StudyMode),
                    c => c.ResolvedDeliveryMode);
                RuleFor(c => c.ResolvedAttendancePattern).AttendancePattern(
                    attendancePatternWasSpecified: t => !string.IsNullOrEmpty(t.AttendancePattern),
                    c => c.ResolvedDeliveryMode);
            }
        }
        internal class NonLarsCourseUploadRowValidator : AbstractValidator<ParsedCsvNonLarsCourseRow>
        {
            public NonLarsCourseUploadRowValidator(
                IClock clock,
                Guid? matchedVenueId)
            {
                RuleFor(c => c.ResolvedCourseType).CourseType();
                RuleFor(c => c.ResolvedSector).Sector();
                RuleFor(c => c.ResolvedEducationLevel).EducationLevel();
                RuleFor(c => c.AwardingBody).AwardingBody();
                RuleFor(c => c.WhoThisCourseIsFor).WhoThisCourseIsFor();
                RuleFor(c => c.EntryRequirements).EntryRequirements();
                RuleFor(c => c.WhatYouWillLearn).WhatYouWillLearn();
                RuleFor(c => c.HowYouWillLearn).HowYouWillLearn();
                RuleFor(c => c.WhatYouWillNeedToBring).WhatYouWillNeedToBring();
                RuleFor(c => c.HowYouWillBeAssessed).HowYouWillBeAssessed();
                RuleFor(c => c.WhereNext).WhereNext();
                RuleFor(c => c.CourseName).CourseName();
                RuleFor(c => c.ProviderCourseRef).ProviderCourseRef();
                RuleFor(c => c.ResolvedDeliveryMode).DeliveryMode();
                RuleFor(c => c.ResolvedStartDate)
                    .Transform(d => d.HasValue ? new DateInput(d.Value) : null)
                    .StartDate(clock.UtcNow, c => c.ResolvedFlexibleStartDate);
                RuleFor(c => c.ResolvedFlexibleStartDate).FlexibleStartDate();
                RuleFor(c => c.VenueName).VenueName(c => c.ResolvedDeliveryMode, c => c.ProviderVenueRef, matchedVenueId);
                RuleFor(c => c.ProviderVenueRef).ProviderVenueRef(c => c.ResolvedDeliveryMode, c => c.VenueName, matchedVenueId);
                RuleFor(c => c.ResolvedNationalDelivery).NationalDelivery(c => c.ResolvedDeliveryMode);
                RuleFor(c => c.ResolvedSubRegions).SubRegions(
                    subRegionsWereSpecified: c => !string.IsNullOrEmpty(c.SubRegions),
                    c => c.ResolvedDeliveryMode,
                    c => c.ResolvedNationalDelivery);
                RuleFor(c => c.CourseWebPage).CourseWebPage();
                RuleFor(c => c.ResolvedCost).Cost(costWasSpecified: c => !string.IsNullOrEmpty(c.Cost), c => c.CostDescription);
                RuleFor(c => c.CostDescription).CostDescription();
                RuleFor(c => c.ResolvedDuration).Duration();
                RuleFor(c => c.ResolvedDurationUnit).DurationUnit();
                RuleFor(c => c.ResolvedStudyMode).StudyMode(
                    studyModeWasSpecified: t => !string.IsNullOrEmpty(t.StudyMode),
                    c => c.ResolvedDeliveryMode);
                RuleFor(c => c.ResolvedAttendancePattern).AttendancePattern(
                    attendancePatternWasSpecified: t => !string.IsNullOrEmpty(t.AttendancePattern),
                    c => c.ResolvedDeliveryMode);
            }
        }
        private static string RemoveASCII(string src)
        {
            string returnstring = string.Empty;
            if (src != null)
                returnstring = Regex.Replace(src, @"[^\u0000-\u007F]", "");
            return returnstring;
        }
    }
}
