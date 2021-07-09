using System;
using System.Collections.Generic;
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

namespace Dfc.CourseDirectory.Core.DataManagement
{
    public partial class FileUploadProcessor
    {
            public async Task<bool> DeleteCourseUploadRowForProvider(Guid providerId, int rowNumber)
            {
                using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher())
                {
                    var courseUpload = await dispatcher.ExecuteQuery(new GetLatestUnpublishedCourseUploadForProvider()
                    {
                        ProviderId = providerId
                    });

                    var existingRows = await dispatcher.ExecuteQuery(new GetCourseUploadRows() { CourseUploadId = courseUpload.CourseUploadId });

                    var rowToDelete = existingRows.SingleOrDefault(x => x.RowNumber == rowNumber);
                    if (rowToDelete == null)    
                    {
                        return false;
                    }
                    var nonDeletedRows = existingRows.Where(x => x.RowNumber != rowNumber).ToArray();

                    var rowCollection = new CourseDataUploadRowInfoCollection(
                        nonDeletedRows
                            .Where(r => r.RowNumber != rowNumber)
                            .Select(r => new CourseDataUploadRowInfo(CsvCourseRow.FromModel(r), r.RowNumber, courseUpload.CourseUploadId)));

                    await ValidateCourseUploadRows(
                        dispatcher,
                        courseUpload.CourseUploadId,
                        courseUpload.ProviderId,
                        rowCollection);

                    await dispatcher.Commit();
                }
                return true;
            }


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

        public async Task<CourseUploadRow> GetCourseUploadRowForProvider(Guid providerId, int rowNumber)
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

                var row = await dispatcher.ExecuteQuery(new GetCourseUploadRow()
                {
                    CourseUploadId = courseUpload.CourseUploadId,
                    RowNumber = rowNumber
                });

                await dispatcher.Commit();

                return row;
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

                var rows = await dispatcher.ExecuteQuery(new GetCourseUploadRows()
                {
                    CourseUploadId = courseUpload.CourseUploadId
                });

                await dispatcher.Commit();

                return (rows, uploadStatus);
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
        internal Guid? FindVenue(ParsedCsvCourseRow row, IReadOnlyCollection<Venue> providerVenues)
        {
            if (row.ResolvedDeliveryMode != CourseDeliveryMode.ClassroomBased)
            {
                return null;
            }

            if (!string.IsNullOrEmpty(row.ProviderVenueRef))
            {
                // N.B. Using `Count()` here instead of `Single()` to protect against bad data where we have duplicates

                var matchedVenues = providerVenues
                    .Where(v => RefMatches(row.ProviderVenueRef, v))
                    .ToArray();

                if (matchedVenues.Length != 1)
                {
                    return null;
                }

                var venue = matchedVenues[0];

                // If VenueName was provided too then it must match
                if (!string.IsNullOrEmpty(row.VenueName) && !NameMatches(row.VenueName, venue))
                {
                    return null;
                }

                return venue.VenueId;
            }

            if (!string.IsNullOrEmpty(row.VenueName))
            {
                // N.B. Using `Count()` here instead of `Single()` to protect against bad data where we have duplicates

                var matchedVenues = providerVenues
                    .Where(v => NameMatches(row.VenueName, v))
                    .ToArray();

                if (matchedVenues.Length != 1)
                {
                    return null;
                }

                return matchedVenues[0].VenueId;
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

            var validLearningAimRefs = await sqlQueryDispatcher.ExecuteQuery(new GetLearningAimRefs()
            {
                LearningAimRefs = rows.Select(r => r.Data.LarsQan).Where(v => !string.IsNullOrWhiteSpace(v))
            });

            var providerVenues = await sqlQueryDispatcher.ExecuteQuery(new GetVenuesByProvider() { ProviderId = providerId });

            var uploadIsValid = true;

            var upsertRecords = new List<UpsertCourseUploadRowsRecord>();

            foreach (var row in rows)
            {
                var rowNumber = row.RowNumber;
                var courseRunId = Guid.NewGuid();

                var parsedRow = ParsedCsvCourseRow.FromCsvCourseRow(row.Data, allRegions);

                var matchedVenueId = FindVenue(parsedRow, providerVenues);

                var validator = new CourseUploadRowValidator(validLearningAimRefs, _clock, matchedVenueId);

                var rowValidationResult = validator.Validate(parsedRow);
                var errors = rowValidationResult.Errors.Select(e => e.ErrorCode).ToArray();
                var rowIsValid = rowValidationResult.IsValid;
                uploadIsValid &= rowIsValid;

                upsertRecords.Add(new UpsertCourseUploadRowsRecord()
                {
                    RowNumber = rowNumber,
                    IsValid = rowIsValid,
                    Errors = errors,
                    CourseId = row.CourseId,
                    CourseRunId = courseRunId,
                    LarsQan = parsedRow.LarsQan,
                    WhoThisCourseIsFor = parsedRow.WhoThisCourseIsFor,
                    EntryRequirements = parsedRow.EntryRequirements,
                    WhatYouWillLearn = parsedRow.WhatYouWillLearn,
                    HowYouWillLearn = parsedRow.HowYouWillLearn,
                    WhatYouWillNeedToBring = parsedRow.WhatYouWillNeedToBring,
                    HowYouWillBeAssessed = parsedRow.HowYouWillBeAssessed,
                    WhereNext = parsedRow.WhereNext,
                    CourseName = parsedRow.CourseName,
                    ProviderCourseRef = parsedRow.ProviderCourseRef,
                    DeliveryMode = parsedRow.DeliveryMode,
                    StartDate = parsedRow.StartDate,
                    FlexibleStartDate = parsedRow.FlexibleStartDate,
                    VenueName = parsedRow.VenueName,
                    ProviderVenueRef = parsedRow.ProviderVenueRef,
                    NationalDelivery = parsedRow.NationalDelivery,
                    SubRegions = parsedRow.SubRegions,
                    CourseWebpage = parsedRow.CourseWebPage,
                    Cost = parsedRow.Cost,
                    CostDescription = parsedRow.CostDescription,
                    Duration = parsedRow.Duration,
                    DurationUnit = parsedRow.DurationUnit,
                    StudyMode = parsedRow.StudyMode,
                    VenueId = matchedVenueId,
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

            await sqlQueryDispatcher.ExecuteQuery(new SetCourseUploadProcessed()
            {
                CourseUploadId = courseUploadId,
                ProcessingCompletedOn = _clock.UtcNow,
                IsValid = uploadIsValid
            });

            var uploadStatus = uploadIsValid ? UploadStatus.ProcessedSuccessfully : UploadStatus.ProcessedWithErrors;

            return (uploadStatus, updatedRows);
        }

        // internal for testing
        internal class CourseUploadRowValidator : AbstractValidator<ParsedCsvCourseRow>
        {
            public CourseUploadRowValidator(
                IReadOnlyCollection<string> validLearningAimRefs,
                IClock clock,
                Guid? matchedVenueId)
            {
                RuleFor(c => c.LarsQan).LarsQan(validLearningAimRefs);
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
                RuleFor(c => c.ResolvedStartDate).StartDate(clock.UtcNow, c => c.ResolvedFlexibleStartDate);
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
                RuleFor(c => c.CostDescription).CostDescription(c => c.ResolvedCost);
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
