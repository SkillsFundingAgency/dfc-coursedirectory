using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using CsvHelper;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Validation.CourseValidation;
using FluentValidation;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataManagement
{
    public partial class FileUploadProcessor
    {
        public async Task DeleteCourseUploadForProvider(Guid providerId)
        {
            using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher())
            {
                var courseUpload = await dispatcher.ExecuteQuery(new GetLatestUnpublishedCourseUploadForProvider()
                {
                    ProviderId = providerId
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

        public async Task<UploadStatus> DeleteCourseUploadRowForProvider(Guid providerId, int rowNumber)
        {
            using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher())
            {
                var courseUpload = await dispatcher.ExecuteQuery(new GetLatestUnpublishedCourseUploadForProvider()
                {
                    ProviderId = providerId
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

        public async Task<UploadStatus> DeleteCourseUploadRowGroupForProvider(Guid providerId, Guid courseId)
        {
            using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher())
            {
                var courseUpload = await dispatcher.ExecuteQuery(new GetLatestUnpublishedCourseUploadForProvider()
                {
                    ProviderId = providerId
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

        public async Task<CourseUploadRowDetail> GetCourseUploadRowDetailForProvider(Guid providerId, int rowNumber)
        {
            using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher())
            {
                var courseUpload = await dispatcher.ExecuteQuery(new GetLatestUnpublishedCourseUploadForProvider()
                {
                    ProviderId = providerId
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
                await RevalidateCourseUploadIfRequired(dispatcher, courseUpload.CourseUploadId);

                var row = await dispatcher.ExecuteQuery(new GetCourseUploadRowDetail()
                {
                    CourseUploadId = courseUpload.CourseUploadId,
                    RowNumber = rowNumber
                });

                await dispatcher.Commit();

                return row;
            }
        }

        public async Task<IReadOnlyCollection<CourseUploadRow>> GetCourseUploadRowGroupForProvider(Guid providerId, Guid courseId)
        {
            using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher())
            {
                var courseUpload = await dispatcher.ExecuteQuery(new GetLatestUnpublishedCourseUploadForProvider()
                {
                    ProviderId = providerId
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
                var uploadStatus = await RevalidateCourseUploadIfRequired(dispatcher, courseUpload.CourseUploadId);

                var rows = await dispatcher.ExecuteQuery(new GetCourseUploadRowsByCourseId()
                {
                    CourseUploadId = courseUpload.CourseUploadId,
                    CourseId = courseId
                });

                await dispatcher.Commit();

                return rows;
            }
        }

        public async Task<(IReadOnlyCollection<CourseUploadRow> Rows, UploadStatus UploadStatus)> GetCourseUploadRowsForProvider(Guid providerId)
        {
            using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher())
            {
                var courseUpload = await dispatcher.ExecuteQuery(new GetLatestUnpublishedCourseUploadForProvider()
                {
                    ProviderId = providerId
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
                var uploadStatus = await RevalidateCourseUploadIfRequired(dispatcher, courseUpload.CourseUploadId);

                var (rows, _) = await dispatcher.ExecuteQuery(new GetCourseUploadRows()
                {
                    CourseUploadId = courseUpload.CourseUploadId
                });

                await dispatcher.Commit();

                return (rows, uploadStatus);
            }
        }

        public async Task<(IReadOnlyCollection<CourseUploadRow> Rows, int TotalRows)> GetCourseUploadRowsWithErrorsForProvider(Guid providerId)
        {
            using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher())
            {
                var courseUpload = await dispatcher.ExecuteQuery(new GetLatestUnpublishedCourseUploadForProvider()
                {
                    ProviderId = providerId
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
                await RevalidateCourseUploadIfRequired(dispatcher, courseUpload.CourseUploadId);

                var (errorRows, totalRows) = await dispatcher.ExecuteQuery(new GetCourseUploadRows()
                {
                    CourseUploadId = courseUpload.CourseUploadId,
                    WithErrorsOnly = true
                });

                await dispatcher.Commit();

                return (errorRows, totalRows);
            }
        }

        public IObservable<UploadStatus> GetCourseUploadStatusUpdatesForProvider(Guid providerId)
        {
            return GetCourseUploadId().ToObservable()
                .SelectMany(courseUploadId => GetCourseUploadStatusUpdates(courseUploadId))
                .DistinctUntilChanged()
                .TakeUntil(status => status.IsTerminal());

            async Task<Guid> GetCourseUploadId()
            {
                using var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher();
                var courseUpload = await dispatcher.ExecuteQuery(new GetLatestUnpublishedCourseUploadForProvider() { ProviderId = providerId });

                if (courseUpload == null)
                {
                    throw new InvalidStateException(InvalidStateReason.NoUnpublishedCourseUpload);
                }

                return courseUpload.CourseUploadId;
            }
        }

        public async Task ProcessCourseFile(Guid courseUploadId, Stream stream)
        {
            using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher())
            {
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

            // At this point `stream` should be a CSV that's already known to conform to `CsvCourseRow`'s schema.
            // We read all the rows upfront because validation needs to group rows into courses.
            // We also don't expect massive files here so reading everything into memory is ok.
            List<CsvCourseRow> rows;
            using (var streamReader = new StreamReader(stream))
            using (var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture))
            {
                rows = await csvReader.GetRecordsAsync<CsvCourseRow>().ToListAsync();
            }

            var rowsCollection = CreateCourseDataUploadRowInfoCollection();

            using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher())
            {
                var venueUpload = await dispatcher.ExecuteQuery(new GetCourseUpload() { CourseUploadId = courseUploadId });
                var providerId = venueUpload.ProviderId;

                await ValidateCourseUploadRows(dispatcher, courseUploadId, providerId, rowsCollection);

                await dispatcher.Commit();
            }

            await DeleteBlob();

            Task DeleteBlob()
            {
                var blobName = $"{Constants.CoursesFolder}/{courseUploadId}.csv";
                return _blobContainerClient.DeleteBlobIfExistsAsync(blobName);
            }

            CourseDataUploadRowInfoCollection CreateCourseDataUploadRowInfoCollection()
            {
                var rowInfos = new List<CourseDataUploadRowInfo>();

                foreach (var group in CsvCourseRow.GroupRows(rows))
                {
                    var courseId = Guid.NewGuid();

                    foreach (var row in group)
                    {
                        rowInfos.Add(new CourseDataUploadRowInfo(row, rowNumber: rowInfos.Count + 2, courseId));
                    }
                }

                return new CourseDataUploadRowInfoCollection(rowInfos);
            }
        }

        public async Task<SaveFileResult> SaveCourseFile(Guid providerId, Stream stream, UserInfo uploadedBy)
        {
            CheckStreamIsProcessable(stream);

            if (await FileIsEmpty(stream))
            {
                return SaveFileResult.EmptyFile();
            }

            if (!await LooksLikeCsv(stream))
            {
                return SaveFileResult.InvalidFile();
            }

            var (fileMatchesSchemaResult, missingHeaders) = await FileMatchesSchema<CsvCourseRow>(stream);
            if (fileMatchesSchemaResult == FileMatchesSchemaResult.InvalidHeader)
            {
                return SaveFileResult.InvalidHeader(missingHeaders);
            }

            var courseUploadId = Guid.NewGuid();

            using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher())
            {
                // Check there isn't an existing unprocessed upload for this provider

                var existingUpload = await dispatcher.ExecuteQuery(new GetLatestUnpublishedCourseUploadForProvider()
                {
                    ProviderId = providerId
                });

                if (existingUpload != null && existingUpload.UploadStatus.IsUnprocessed())
                {
                    return SaveFileResult.ExistingFileInFlight();
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
                    ProviderId = providerId
                });

                await dispatcher.Transaction.CommitAsync();
            }

            await UploadToBlobStorage();

            return SaveFileResult.Success(courseUploadId, UploadStatus.Created);

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

        public async Task<UploadStatus> UpdateCourseUploadRowForProvider(Guid providerId, int rowNumber, CourseUploadRowUpdate update)
        {
            using var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher();

            var courseUpload = await dispatcher.ExecuteQuery(new GetLatestUnpublishedCourseUploadForProvider()
            {
                ProviderId = providerId
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

            var updatedRows = new CourseDataUploadRowInfoCollection(
                new CourseDataUploadRowInfo(
                    new CsvCourseRow()
                    {
                        AttendancePattern = ParsedCsvCourseRow.MapAttendancePattern(update.AttendancePattern),
                        Cost = ParsedCsvCourseRow.MapCost(update.Cost),
                        CostDescription = update.CostDescription,
                        CourseName = update.CourseName,
                        CourseWebPage = update.CourseWebPage,
                        DeliveryMode = ParsedCsvCourseRow.MapDeliveryMode(update.DeliveryMode),
                        Duration = ParsedCsvCourseRow.MapDuration(update.Duration),
                        DurationUnit = ParsedCsvCourseRow.MapDurationUnit(update.DurationUnit),
                        EntryRequirements = row.EntryRequirements,
                        FlexibleStartDate = ParsedCsvCourseRow.MapFlexibleStartDate(update.FlexibleStartDate),
                        HowYouWillBeAssessed = row.HowYouWillBeAssessed,
                        HowYouWillLearn = row.HowYouWillLearn,
                        LearnAimRef = row.LearnAimRef,
                        NationalDelivery = ParsedCsvCourseRow.MapNationalDelivery(update.NationalDelivery),
                        ProviderCourseRef = update.ProviderCourseRef,
                        ProviderVenueRef = venue?.ProviderVenueRef,
                        StartDate = ParsedCsvCourseRow.MapStartDate(update.StartDate),
                        StudyMode = ParsedCsvCourseRow.MapStudyMode(update.StudyMode),
                        SubRegions = ParsedCsvCourseRow.MapSubRegions(update.SubRegionIds, allRegions),
                        VenueName = venue?.VenueName,
                        WhatYouWillLearn = row.WhatYouWillLearn,
                        WhatYouWillNeedToBring = row.WhatYouWillNeedToBring,
                        WhereNext = row.WhereNext,
                        WhoThisCourseIsFor = row.WhoThisCourseIsFor
                    },
                    row.RowNumber,
                    row.CourseId,
                    venue?.VenueId));

            await ValidateCourseUploadRows(dispatcher, courseUpload.CourseUploadId, courseUpload.ProviderId, updatedRows);

            // Other rows not covered by this group may require revalidation;
            // ensure revalidation is done if required so that `uploadStatus` is accurate
            var uploadStatus = await RevalidateCourseUploadIfRequired(dispatcher, courseUpload.CourseUploadId);

            await dispatcher.Commit();

            return uploadStatus;
        }

        public async Task<UploadStatus> UpdateCourseUploadRowGroupForProvider(Guid providerId, Guid courseId, CourseUploadRowGroupUpdate update)
        {
            using var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher();

            var courseUpload = await dispatcher.ExecuteQuery(new GetLatestUnpublishedCourseUploadForProvider()
            {
                ProviderId = providerId
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
                            EntryRequirements = update.EntryRequirements,
                            FlexibleStartDate = r.FlexibleStartDate,
                            HowYouWillBeAssessed = update.HowYouWillBeAssessed,
                            HowYouWillLearn = update.HowYouWillLearn,
                            LearnAimRef = r.LearnAimRef,
                            NationalDelivery = r.NationalDelivery,
                            ProviderCourseRef = r.ProviderCourseRef,
                            ProviderVenueRef = r.ProviderVenueRef,
                            StartDate = r.StartDate,
                            StudyMode = r.StudyMode,
                            SubRegions = r.SubRegions,
                            VenueName = r.VenueName,
                            WhatYouWillLearn = update.WhatYouWillLearn,
                            WhatYouWillNeedToBring = update.WhatYouWillNeedToBring,
                            WhereNext = update.WhereNext,
                            WhoThisCourseIsFor = update.WhoThisCourseIsFor
                        },
                        r.RowNumber,
                        courseId)));

            await ValidateCourseUploadRows(dispatcher, courseUpload.CourseUploadId, courseUpload.ProviderId, updatedRows);

            // Other rows not covered by this group may require revalidation;
            // ensure revalidation is done if required so that `uploadStatus` is accurate
            var uploadStatus = await RevalidateCourseUploadIfRequired(dispatcher, courseUpload.CourseUploadId);

            await dispatcher.Commit();

            return uploadStatus;
        }

        protected async Task<UploadStatus> GetCourseUploadStatus(Guid courseUploadId)
        {
            using var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher();
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
        internal Venue FindVenue(CourseDataUploadRowInfo row, IReadOnlyCollection<Venue> providerVenues)
        {
            if (row.VenueIdHint.HasValue)
            {
                return providerVenues.Single(v => v.VenueId == row.VenueIdHint);
            }

            if (!string.IsNullOrEmpty(row.Data.ProviderVenueRef))
            {
                // N.B. Using `Count()` here instead of `Single()` to protect against bad data where we have duplicates

                var matchedVenues = providerVenues
                    .Where(v => RefMatches(row.Data.ProviderVenueRef, v))
                    .ToArray();

                if (matchedVenues.Length != 1)
                {
                    return null;
                }

                var venue = matchedVenues[0];

                // If VenueName was provided too then it must match
                if (!string.IsNullOrEmpty(row.Data.VenueName) && !NameMatches(row.Data.VenueName, venue))
                {
                    return null;
                }

                return venue;
            }

            if (!string.IsNullOrEmpty(row.Data.VenueName))
            {
                // N.B. Using `Count()` here instead of `Single()` to protect against bad data where we have duplicates

                var matchedVenues = providerVenues
                    .Where(v => NameMatches(row.Data.VenueName, v))
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
            Guid courseUploadId)
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

            var rowsCollection = new CourseDataUploadRowInfoCollection(
                toBeRevalidated.Select(r => new CourseDataUploadRowInfo(CsvCourseRow.FromModel(r), r.RowNumber, r.CourseId)));

            var (uploadStatus, _) = await ValidateCourseUploadRows(sqlQueryDispatcher, courseUploadId, courseUpload.ProviderId, rowsCollection);

            return uploadStatus;
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

                var matchedVenue = FindVenue(row, providerVenues);

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
                    WhoThisCourseIsFor = parsedRow.WhoThisCourseIsFor,
                    EntryRequirements = parsedRow.EntryRequirements,
                    WhatYouWillLearn = parsedRow.WhatYouWillLearn,
                    HowYouWillLearn = parsedRow.HowYouWillLearn,
                    WhatYouWillNeedToBring = parsedRow.WhatYouWillNeedToBring,
                    HowYouWillBeAssessed = parsedRow.HowYouWillBeAssessed,
                    WhereNext = parsedRow.WhereNext,
                    CourseName = parsedRow.CourseName,
                    ProviderCourseRef = parsedRow.ProviderCourseRef,
                    DeliveryMode = ParsedCsvCourseRow.MapDeliveryMode(parsedRow.ResolvedDeliveryMode) ?? parsedRow.DeliveryMode,
                    StartDate = ParsedCsvCourseRow.MapStartDate(parsedRow.ResolvedStartDate) ?? parsedRow.StartDate,
                    FlexibleStartDate = ParsedCsvCourseRow.MapFlexibleStartDate(parsedRow.ResolvedFlexibleStartDate) ?? parsedRow.FlexibleStartDate,
                    VenueName = matchedVenue?.VenueName,
                    ProviderVenueRef = matchedVenue?.ProviderVenueRef,
                    NationalDelivery = ParsedCsvCourseRow.MapNationalDelivery(parsedRow.ResolvedNationalDelivery) ?? parsedRow.NationalDelivery,
                    SubRegions = parsedRow.SubRegions,
                    CourseWebpage = parsedRow.CourseWebPage,
                    Cost = ParsedCsvCourseRow.MapCost(parsedRow.ResolvedCost) ?? parsedRow.Cost,
                    CostDescription = parsedRow.CostDescription,
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
                    ResolvedSubRegions = parsedRow.ResolvedSubRegions?.Select(sr => sr.Id)?.ToArray()
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
    }
}
