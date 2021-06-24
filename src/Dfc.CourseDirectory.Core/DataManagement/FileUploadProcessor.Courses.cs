using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

            var rowsCollection = new CourseDataUploadRowInfoCollection(
                rows: rows.Select((r, i) => new CourseDataUploadRowInfo(r, rowNumber: i + 2)));

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

                var rows = (await dispatcher.ExecuteQuery(new GetCourseUploadRows() { CourseUploadId = courseUpload.CourseUploadId }));
                return (rows, courseUpload.UploadStatus);
            }
        }

        // internal for testing
        internal async Task<(UploadStatus uploadStatus, IReadOnlyCollection<CourseUploadRow> Rows)> ValidateCourseUploadRows(
            ISqlQueryDispatcher sqlQueryDispatcher,
            Guid courseUploadId,
            Guid providerId,
            CourseDataUploadRowInfoCollection rows)
        {
            var regions = await _regionCache.GetAllRegions();

            var validLearningAimRefs = await sqlQueryDispatcher.ExecuteQuery(new GetLearningAimRefs()
            {
                LearningAimRefs = rows.Select(r => r.Data.LarsQan).Where(v => !string.IsNullOrWhiteSpace(v))
            });

            var providerVenues = await sqlQueryDispatcher.ExecuteQuery(new GetVenuesByProvider() { ProviderId = providerId });

            var uploadIsValid = true;
            var validator = new CourseUploadRowValidator(regions, validLearningAimRefs, _clock, providerVenues);

            // Group rows into courses
            var courseGroups = rows.GroupBy(r => r.Data, new CsvCourseRowCourseComparer());

            var upsertRecords = new List<SetCourseUploadRowsRecord>();

            foreach (var group in courseGroups)
            {
                var courseId = Guid.NewGuid();

                foreach (var row in group)
                {
                    var rowNumber = row.RowNumber;
                    var courseRunId = Guid.NewGuid();

                    var rowValidationResult = validator.Validate(row.Data);
                    var errors = rowValidationResult.Errors.Select(e => e.ErrorCode).ToArray();
                    var rowIsValid = rowValidationResult.IsValid;
                    uploadIsValid &= rowIsValid;

                    upsertRecords.Add(new SetCourseUploadRowsRecord()
                    {
                        RowNumber = rowNumber,
                        IsValid = rowIsValid,
                        Errors = errors,
                        CourseId = courseId,
                        CourseRunId = courseRunId,
                        LarsQan = row.Data.LarsQan,
                        WhoThisCourseIsFor = row.Data.WhoThisCourseIsFor,
                        EntryRequirements = row.Data.EntryRequirements,
                        WhatYouWillLearn = row.Data.WhatYouWillLearn,
                        HowYouWillLearn = row.Data.HowYouWillLearn,
                        WhatYouWillNeedToBring = row.Data.WhatYouWillNeedToBring,
                        HowYouWillBeAssessed = row.Data.HowYouWillBeAssessed,
                        WhereNext = row.Data.WhereNext,
                        CourseName = row.Data.CourseName,
                        ProviderCourseRef = row.Data.ProviderCourseRef,
                        DeliveryMode = row.Data.DeliveryMode,
                        StartDate = row.Data.StartDate,
                        FlexibleStartDate = row.Data.FlexibleStartDate,
                        VenueName = row.Data.VenueName,
                        ProviderVenueRef = row.Data.ProviderVenueRef,
                        NationalDelivery = row.Data.NationalDelivery,
                        SubRegions = row.Data.SubRegions,
                        CourseWebpage = row.Data.CourseWebPage,
                        Cost = row.Data.Cost,
                        CostDescription = row.Data.CostDescription,
                        Duration = row.Data.Duration,
                        DurationUnit = row.Data.DurationUnit,
                        StudyMode = row.Data.StudyMode,
                        AttendancePattern = row.Data.AttendancePattern
                    });
                }
            }

            var updatedRows = await sqlQueryDispatcher.ExecuteQuery(new SetCourseUploadRows()
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

        // virtual for testing
        protected virtual IObservable<UploadStatus> GetCourseUploadStatusUpdates(Guid courseUploadId) =>
            Observable.Interval(_statusUpdatesPollInterval)
                .SelectMany(_ => Observable.FromAsync(() => GetCourseUploadStatus(courseUploadId)));

        internal class CourseUploadRowValidator : AbstractValidator<CsvCourseRow>
        {
            public CourseUploadRowValidator(
                IReadOnlyCollection<Region> allRegions,
                IReadOnlyCollection<string> validLearningAimRefs,
                IClock clock,
                IReadOnlyCollection<Venue> providerVenues)
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
                RuleFor(c => c.DeliveryMode).DeliveryMode();
                RuleFor(c => c.StartDate).StartDate(clock.UtcNow, c => c.FlexibleStartDate);
                RuleFor(c => c.FlexibleStartDate).FlexibleStartDate();
                RuleFor(c => c.VenueName).VenueName(c => c.DeliveryMode, c => c.ProviderVenueRef, providerVenues);
                RuleFor(c => c.ProviderVenueRef).ProviderVenueRef(c => c.DeliveryMode, c => c.VenueName, providerVenues);
                RuleFor(c => c.NationalDelivery).NationalDelivery(c => c.DeliveryMode);
                RuleFor(c => c.SubRegions).SubRegions(c => c.DeliveryMode, c => c.NationalDelivery, allRegions);
                RuleFor(c => c.CourseWebPage).CourseWebPage();
                RuleFor(c => c.Cost).Cost(c => c.CostDescription);
                RuleFor(c => c.CostDescription).CostDescription(c => c.Cost);
                RuleFor(c => c.Duration).Duration();
                RuleFor(c => c.DurationUnit).DurationUnit();
                RuleFor(c => c.StudyMode).StudyMode(c => c.DeliveryMode);
                RuleFor(c => c.AttendancePattern).AttendancePattern(c => c.DeliveryMode);
            }
        }

        private class CsvCourseRowCourseComparer : IEqualityComparer<CsvCourseRow>
        {
            public bool Equals([AllowNull] CsvCourseRow x, [AllowNull] CsvCourseRow y)
            {
                if (x is null && y is null)
                {
                    return true;
                }

                if (x is null || y is null)
                {
                    return false;
                }

                // Don't group together records that have no LARS code
                if (string.IsNullOrEmpty(x.LarsQan) || string.IsNullOrEmpty(y.LarsQan))
                {
                    return false;
                }

                return
                    x.LarsQan == y.LarsQan &&
                    x.WhoThisCourseIsFor == y.WhoThisCourseIsFor &&
                    x.EntryRequirements == y.EntryRequirements &&
                    x.WhatYouWillLearn == y.WhatYouWillLearn &&
                    x.HowYouWillLearn == y.HowYouWillLearn &&
                    x.WhatYouWillNeedToBring == y.WhatYouWillNeedToBring &&
                    x.HowYouWillBeAssessed == y.HowYouWillBeAssessed &&
                    x.WhereNext == y.WhereNext;
            }

            public int GetHashCode([DisallowNull] CsvCourseRow obj) =>
                HashCode.Combine(
                    obj.LarsQan,
                    obj.WhoThisCourseIsFor,
                    obj.EntryRequirements,
                    obj.WhatYouWillLearn,
                    obj.HowYouWillLearn,
                    obj.WhatYouWillNeedToBring,
                    obj.HowYouWillBeAssessed,
                    obj.WhereNext);
        }
    }
}
