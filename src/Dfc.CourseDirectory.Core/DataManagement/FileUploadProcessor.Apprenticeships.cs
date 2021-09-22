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
using Dfc.CourseDirectory.Core.Validation;
using Dfc.CourseDirectory.Core.Validation.ApprenticeshipValidation;
using FluentValidation;

namespace Dfc.CourseDirectory.Core.DataManagement
{
    public partial class FileUploadProcessor
    {
        public async Task<SaveApprenticeshipFileResult> SaveApprenticeshipFile(Guid providerId, Stream stream, UserInfo uploadedBy)
        {
            CheckStreamIsProcessable(stream);

            if (await FileIsEmpty(stream))
            {
                return SaveApprenticeshipFileResult.EmptyFile();
            }

            if (!await LooksLikeCsv(stream))
            {
                return SaveApprenticeshipFileResult.InvalidFile();
            }

            var (fileMatchesSchemaResult, missingHeaders) = await FileMatchesSchema<CsvApprenticeshipRow>(stream);
            if (fileMatchesSchemaResult == FileMatchesSchemaResult.InvalidHeader)
            {
                return SaveApprenticeshipFileResult.InvalidHeader(missingHeaders);
            }

            var (missingStandards, invalidStandards) = await ValidateStandardCodes(stream);
            if (missingStandards.Length > 0 || invalidStandards.Length > 0)
            {
                return SaveApprenticeshipFileResult.InvalidStandards(missingStandards, invalidStandards);
            }

            var apprenticeshipUploadId = Guid.NewGuid();

            using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher(System.Data.IsolationLevel.ReadCommitted))
            {
                await AcquireExclusiveApprenticeshipUploadLockForProvider(providerId, dispatcher);

                // Check there isn't an existing unprocessed upload for this provider
                var existingUpload = await dispatcher.ExecuteQuery(new GetLatestUnpublishedApprenticeshipUploadForProvider()
                {
                    ProviderId = providerId
                });

                if (existingUpload != null && existingUpload.UploadStatus.IsUnprocessed())
                {
                    return SaveApprenticeshipFileResult.ExistingFileInFlight();
                }

                // Abandon any existing un-published upload (there will be one at most)
                if (existingUpload != null)
                {
                    await dispatcher.ExecuteQuery(new SetApprenticeshipUploadAbandoned()
                    {
                        ApprenticeshipUploadId = existingUpload.ApprenticeshipUploadId,
                        AbandonedOn = _clock.UtcNow
                    });
                }

                await dispatcher.ExecuteQuery(new CreateApprenticeshipUpload()
                {
                    ApprenticeshipUploadId = apprenticeshipUploadId,
                    CreatedBy = uploadedBy,
                    CreatedOn = _clock.UtcNow,
                    ProviderId = providerId
                });

                await dispatcher.Transaction.CommitAsync();
            }

            await UploadToBlobStorage();

            return SaveApprenticeshipFileResult.Success(apprenticeshipUploadId, UploadStatus.Created);

            async Task UploadToBlobStorage()
            {
                if (!_containerIsKnownToExist)
                {
                    await _blobContainerClient.CreateIfNotExistsAsync();
                    _containerIsKnownToExist = true;
                }

                var blobName = $"{Constants.ApprenticeshipsFolder}/{apprenticeshipUploadId}.csv";
                await _blobContainerClient.UploadBlobAsync(blobName, stream);
            }
        }

        public async Task ProcessApprenticeshipFile(Guid apprenticeshipUploadId, Stream stream)
        {
            using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher(System.Data.IsolationLevel.ReadCommitted))
            {
                var setProcessingResult = await dispatcher.ExecuteQuery(new SetApprenticeshipUploadProcessing()
                {
                    ApprenticeshipUploadId = apprenticeshipUploadId,
                    ProcessingStartedOn = _clock.UtcNow
                });

                if (setProcessingResult != SetApprenticeshipUploadProcessingResult.Success)
                {
                    await DeleteBlob();

                    return;
                }

                await dispatcher.Commit();
            }

            List<CsvApprenticeshipRow> rows;
            using (var streamReader = new StreamReader(stream))
            using (var csvReader = new CsvHelper.CsvReader(streamReader, CultureInfo.InvariantCulture))
            {
                rows = await csvReader.GetRecordsAsync<CsvApprenticeshipRow>().ToListAsync();
            }

            var rowsCollection = CreateApprenticeshipDataUploadRowInfoCollection();

            using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher(System.Data.IsolationLevel.ReadCommitted))
            {
                var venueUpload = await dispatcher.ExecuteQuery(new GetApprenticeshipUpload() { ApprenticeshipUploadId = apprenticeshipUploadId });
                var providerId = venueUpload.ProviderId;

                await AcquireExclusiveApprenticeshipUploadLockForProvider(providerId, dispatcher);
                await ValidateApprenticeshipUploadRows(dispatcher, apprenticeshipUploadId, providerId, rowsCollection);

                await dispatcher.Commit();
            }

            await DeleteBlob();

            Task DeleteBlob()
            {
                var blobName = $"{Constants.ApprenticeshipsFolder}/{apprenticeshipUploadId}.csv";
                return _blobContainerClient.DeleteBlobIfExistsAsync(blobName);
            }

            ApprenticeshipDataUploadRowInfoCollection CreateApprenticeshipDataUploadRowInfoCollection()
            {
                // N.B. It's important we maintain ordering here; RowNumber needs to match the input

                var grouped = CsvApprenticeshipRow.GroupRows(rows);
                var groupApprenticeshipIds = grouped.Select(g => (ApprenticeshipId: Guid.NewGuid(), Rows: g)).ToArray();

                var rowInfos = new List<ApprenticeshipDataUploadRowInfo>(rows.Count);

                foreach (var row in rows)
                {
                    var apprenticeshipId = groupApprenticeshipIds.Single(g => g.Rows.Contains(row)).ApprenticeshipId;

                    rowInfos.Add(new ApprenticeshipDataUploadRowInfo(row, rowNumber: rowInfos.Count + 2, apprenticeshipId));
                }

                return new ApprenticeshipDataUploadRowInfoCollection(rowInfos);
            }
        }

        public async Task<PublishResult> PublishApprenticeshipUploadForProvider(Guid providerId, UserInfo publishedBy)
        {
            using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher(System.Data.IsolationLevel.ReadCommitted))
            {
                await AcquireExclusiveApprenticeshipUploadLockForProvider(providerId, dispatcher);

                var apprenticeshipUpload = await dispatcher.ExecuteQuery(new GetLatestUnpublishedApprenticeshipUploadForProvider()
                {
                    ProviderId = providerId
                });

                if (apprenticeshipUpload == null)
                {
                    throw new InvalidStateException(InvalidStateReason.NoUnpublishedApprenticeshipUpload);
                }

                if (apprenticeshipUpload.UploadStatus.IsUnprocessed())
                {
                    throw new InvalidUploadStatusException(
                        apprenticeshipUpload.UploadStatus,
                        UploadStatus.ProcessedWithErrors,
                        UploadStatus.ProcessedSuccessfully);
                }

                if (apprenticeshipUpload.UploadStatus == UploadStatus.ProcessedWithErrors)
                {
                    return PublishResult.UploadHasErrors();
                }

                var uploadStatus = await RevalidateApprenticeshipUploadIfRequired(dispatcher, apprenticeshipUpload.ApprenticeshipUploadId);

                if (uploadStatus == UploadStatus.ProcessedWithErrors)
                {
                    return PublishResult.UploadHasErrors();
                }

                var publishedOn = _clock.UtcNow;

                var publishResult = await dispatcher.ExecuteQuery(new PublishApprenticeshipUpload()
                {
                    ApprenticeshipUploadId = apprenticeshipUpload.ApprenticeshipUploadId,
                    PublishedBy = publishedBy,
                    PublishedOn = publishedOn
                });

                await dispatcher.Commit();

                Debug.Assert(publishResult.IsT1);
                var publishedApprenticeshipsCount = publishResult.AsT1.PublishedCount;

                return PublishResult.Success(publishedApprenticeshipsCount);
            }
        }

        public IObservable<UploadStatus> GetApprenticeshipUploadStatusUpdatesForProvider(Guid providerId)
        {
            return GetApprenticeshipUploadId().ToObservable()
                .SelectMany(apprenticeshipUploadId => GetApprenticeshipUploadStatusUpdates(apprenticeshipUploadId))
                .DistinctUntilChanged()
                .TakeUntil(status => status.IsTerminal());

            async Task<Guid> GetApprenticeshipUploadId()
            {
                using var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher(System.Data.IsolationLevel.ReadCommitted);
                var apprenticeshipUpload = await dispatcher.ExecuteQuery(new GetLatestUnpublishedApprenticeshipUploadForProvider() { ProviderId = providerId });

                if (apprenticeshipUpload == null)
                {
                    throw new InvalidStateException(InvalidStateReason.NoUnpublishedApprenticeshipUpload);
                }

                return apprenticeshipUpload.ApprenticeshipUploadId;
            }
        }

        protected virtual IObservable<UploadStatus> GetApprenticeshipUploadStatusUpdates(Guid apprenticeshipUploadId) =>
            Observable.Interval(_statusUpdatesPollInterval)
                .SelectMany(_ => Observable.FromAsync(() => GetApprenticeshipUploadStatus(apprenticeshipUploadId)));

        protected async Task<UploadStatus> GetApprenticeshipUploadStatus(Guid apprenticeshipUploadId)
        {
            using var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher(System.Data.IsolationLevel.ReadCommitted);
            var apprenticeshipUpload = await dispatcher.ExecuteQuery(new GetApprenticeshipUpload() { ApprenticeshipUploadId = apprenticeshipUploadId });

            if (apprenticeshipUpload == null)
            {
                throw new ArgumentException("Specified apprenticeship upload does not exist.", nameof(apprenticeshipUploadId));
            }

            return apprenticeshipUpload.UploadStatus;
        }

        // internal for testing
        internal async Task<(UploadStatus uploadStatus, IReadOnlyCollection<ApprenticeshipUploadRow> Rows)> ValidateApprenticeshipUploadRows(
            ISqlQueryDispatcher sqlQueryDispatcher,
            Guid apprenticeshipUploadId,
            Guid providerId,
            ApprenticeshipDataUploadRowInfoCollection rows)
        {
            var rowsAreValid = true;

            var providerVenues = await sqlQueryDispatcher.ExecuteQuery(new GetVenuesByProvider() { ProviderId = providerId });

            var allRegions = await _regionCache.GetAllRegions();

            var upsertRecords = new List<UpsertApprenticeshipUploadRowsRecord>();
            var allRows = rows.Select(x => ParsedCsvApprenticeshipRow.FromCsvApprenticeshipRow(x.Data, allRegions));

            foreach (var row in rows)
            {
                var rowNumber = row.RowNumber;
                var apprenticeshipLocationId = Guid.NewGuid();

                var parsedRow = ParsedCsvApprenticeshipRow.FromCsvApprenticeshipRow(row.Data, allRegions);

                var matchedVenue = FindVenue(row, providerVenues);

                var validator = new ApprenticeshipUploadRowValidator(matchedVenue?.VenueId, allRows.ToList());

                var rowValidationResult = validator.Validate(parsedRow);
                var errors = rowValidationResult.Errors.Select(e => e.ErrorCode).ToArray();
                var rowIsValid = rowValidationResult.IsValid;
                rowsAreValid &= rowIsValid;

                upsertRecords.Add(new UpsertApprenticeshipUploadRowsRecord()
                {
                    RowNumber = rowNumber,
                    IsValid = rowIsValid,
                    Errors = errors,
                    ApprenticeshipId = row.ApprenticeshipId,
                    ApprenticeshipLocationId = apprenticeshipLocationId,
                    StandardCode = int.Parse(parsedRow.StandardCode),
                    StandardVersion = int.Parse(parsedRow.StandardVersion),
                    ApprenticeshipInformation = parsedRow.ApprenticeshipInformation,
                    ApprenticeshipWebpage = parsedRow.ApprenticeshipWebpage,
                    ContactEmail = parsedRow.ContactEmail,
                    ContactPhone = parsedRow.ContactPhone,
                    ContactUrl = parsedRow.ContactUrl,
                    DeliveryMethod = ParsedCsvApprenticeshipRow.MapDeliveryMethod(parsedRow.ResolvedDeliveryMethod),
                    VenueName = matchedVenue?.VenueName ?? parsedRow.VenueName,
                    YourVenueReference = parsedRow.YourVenueReference,
                    Radius = parsedRow.Radius,
                    DeliveryModes = ParsedCsvApprenticeshipRow.MapDeliveryModes(parsedRow.ResolvedDeliveryModes),
                    NationalDelivery = ParsedCsvApprenticeshipRow.MapNationalDelivery(parsedRow.ResolvedNationalDelivery),
                    SubRegions = parsedRow.SubRegion,
                    VenueId = matchedVenue?.VenueId,
                    ResolvedSubRegions = parsedRow.ResolvedSubRegions?.Select(sr => sr.Id)?.ToArray(),
                    ResolvedDeliveryMethod = parsedRow.ResolvedDeliveryMethod,
                    ResolvedDeliveryModes = parsedRow.ResolvedDeliveryModes,
                    ResolvedNationalDelivery = parsedRow.ResolvedNationalDelivery,
                    ResolvedRadius = parsedRow.ResolvedRadius
                });
            }

            var updatedRows = await sqlQueryDispatcher.ExecuteQuery(new UpsertApprenticeshipUploadRows()
            {
                ApprenticeshipUploadId = apprenticeshipUploadId,
                ValidatedOn = _clock.UtcNow,
                Records = upsertRecords
            });

            // If all the provided rows are valid check if there are any more invalid rows
            var uploadIsValid = rowsAreValid ?
                (await sqlQueryDispatcher.ExecuteQuery(new GetApprenticeshipUploadInvalidRowCount() { ApprenticeshipUploadId = apprenticeshipUploadId })) == 0 :
                false;

            await sqlQueryDispatcher.ExecuteQuery(new SetApprenticeshipUploadProcessed()
            {
                ApprenticeshipUploadId = apprenticeshipUploadId,
                ProcessingCompletedOn = _clock.UtcNow,
                IsValid = uploadIsValid
            });

            var uploadStatus = await RefreshApprenticeshipUploadValidationStatus(apprenticeshipUploadId, sqlQueryDispatcher);

            return (uploadStatus, updatedRows);
        }

        public async Task<IReadOnlyCollection<ApprenticeshipUploadRow>> GetApprenticeshipUploadRowsWithErrorsForProvider(Guid providerId)
        {
            using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher(System.Data.IsolationLevel.ReadCommitted))
            {
                await AcquireExclusiveApprenticeshipUploadLockForProvider(providerId, dispatcher);

                var apprenticeshipUpload = await dispatcher.ExecuteQuery(new GetLatestUnpublishedApprenticeshipUploadForProvider()
                {
                    ProviderId = providerId
                });

                if (apprenticeshipUpload == null)
                {
                    throw new InvalidStateException(InvalidStateReason.NoUnpublishedCourseUpload);
                }

                if (apprenticeshipUpload.UploadStatus != UploadStatus.ProcessedSuccessfully &&
                    apprenticeshipUpload.UploadStatus != UploadStatus.ProcessedWithErrors)
                {
                    throw new InvalidUploadStatusException(
                        apprenticeshipUpload.UploadStatus,
                        UploadStatus.ProcessedSuccessfully,
                        UploadStatus.ProcessedWithErrors);
                }

                // If the world around us has changed (apprenticeship added etc.) then we might need to revalidate
                await RevalidateApprenticeshipUploadIfRequired(dispatcher, apprenticeshipUpload.ApprenticeshipUploadId);

                var (errorRows, totalRows) = await dispatcher.ExecuteQuery(new GetApprenticeshipUploadRows()
                {
                    ApprenticeshipUploadId = apprenticeshipUpload.ApprenticeshipUploadId,
                    WithErrorsOnly = true
                });

                await dispatcher.Commit();

                return errorRows;
            }
        }

        internal async Task<UploadStatus> RevalidateApprenticeshipUploadIfRequired(
            ISqlQueryDispatcher sqlQueryDispatcher,
            Guid apprenticeshipUploadId)
        {
            var apprenticeshipUpload = await sqlQueryDispatcher.ExecuteQuery(new GetApprenticeshipUpload() { ApprenticeshipUploadId = apprenticeshipUploadId });

            if (apprenticeshipUpload == null)
            {
                throw new ArgumentException("Apprenticeship upload does not exist.", nameof(apprenticeshipUploadId));
            }

            var toBeRevalidated = await GetApprenticeshipUploadRowsRequiringRevalidation(sqlQueryDispatcher, apprenticeshipUpload);

            if (toBeRevalidated.Count == 0)
            {
                return apprenticeshipUpload.UploadStatus;
            }

            var rowsCollection = new ApprenticeshipDataUploadRowInfoCollection(
                toBeRevalidated.Select(r => new ApprenticeshipDataUploadRowInfo(CsvApprenticeshipRow.FromModel(r), r.RowNumber, r.ApprenticeshipId)));

            var (uploadStatus, _) = await ValidateApprenticeshipUploadRows(sqlQueryDispatcher, apprenticeshipUploadId, apprenticeshipUpload.ApprenticeshipUploadId, rowsCollection);

            return uploadStatus;
        }

        // internal for testing
        internal async Task<IReadOnlyCollection<ApprenticeshipUploadRow>> GetApprenticeshipUploadRowsRequiringRevalidation(
            ISqlQueryDispatcher sqlQueryDispatcher,
            ApprenticeshipUpload apprenticeshipUpload)
        {
            if (apprenticeshipUpload.UploadStatus != UploadStatus.ProcessedWithErrors &&
                apprenticeshipUpload.UploadStatus != UploadStatus.ProcessedSuccessfully)
            {
                throw new InvalidOperationException($"Apprenticeship upload at status {apprenticeshipUpload.UploadStatus} cannot be revalidated.");
            }

            // We need to revalidate any rows that are linked to venues where either
            // the linked venue has been amended/deleted (it may not match now)
            // or where a new venue has been added (there may be a match where there wasn't before).
            //
            // Note this is a different approach to venues (where we have to revalidate the entire file);
            // for courses we want to minimize the amount of data we're shuttling back and forth from the DB.

            return await sqlQueryDispatcher.ExecuteQuery(
                new GetApprenticeshipUploadRowsToRevalidate() { ApprenticeshipUploadId = apprenticeshipUpload.ApprenticeshipUploadId });
        }


        internal async Task<(int[] Missing, (int StandardCode, int StandardVersion, int RowNumber)[] Invalid)> ValidateStandardCodes(Stream stream)
        {
            CheckStreamIsProcessable(stream);

            try
            {
                using (var streamReader = new StreamReader(stream, leaveOpen: true))
                using (var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture))
                using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher(System.Data.IsolationLevel.ReadCommitted))
                {
                    await csvReader.ReadAsync();
                    csvReader.ReadHeader();

                    var rowStandards = csvReader.GetRecords<CsvApprenticeshipRow>()
                        .Select((row, i) =>
                        {
                            int? standardCode = int.TryParse(row.StandardCode, out var parsed) ? parsed : (int?)null;
                            int? standardVersion = int.TryParse(row.StandardVersion, out parsed) ? parsed : (int?)null;

                            return (RowNumber: i + 2, StandardCode: standardCode, StandardVersion: standardVersion);
                        })
                        .ToArray();

                    var standards = await dispatcher.ExecuteQuery(new GetStandards()
                    {
                        StandardCodes = rowStandards
                            .Where(r => r.StandardCode.HasValue && r.StandardVersion.HasValue)
                            .Select(r => (r.StandardCode.Value, r.StandardVersion.Value))
                            .Distinct()
                    });

                    var missing = rowStandards
                        .Where(r => !r.StandardCode.HasValue || !r.StandardVersion.HasValue)
                        .Select(r => r.RowNumber)
                        .ToArray();

                    var invalid = rowStandards
                        .Where(r => r.StandardCode.HasValue && r.StandardVersion.HasValue)
                        .Select(r => (StandardCode: r.StandardCode.Value, StandardVersion: r.StandardVersion.Value, r.RowNumber))
                        .Where(r => !standards.ContainsKey((r.StandardCode, r.StandardVersion)))
                        .ToArray();

                    return (missing, invalid);
                }
            }
            finally
            {
                stream.Seek(0L, SeekOrigin.Begin);
            }
        }

        private async Task AcquireExclusiveApprenticeshipUploadLockForProvider(Guid providerId, ISqlQueryDispatcher sqlQueryDispatcher)
        {
            var lockName = $"DM_Apprenticeship:{providerId}";
            const int timeoutMilliseconds = 3000;

            var acquired = await sqlQueryDispatcher.ExecuteQuery(new GetExclusiveLock()
            {
                Name = lockName,
                TimeoutMilliseconds = timeoutMilliseconds
            });

            if (!acquired)
            {
                throw new Exception($"Failed to acquire exclusive apprenticeship upload lock for provider {providerId}.");
            }
        }

        public async Task<(IReadOnlyCollection<ApprenticeshipUploadRow> Rows, UploadStatus UploadStatus)> GetApprenticeshipUploadRowsForProvider(Guid providerId)
        {
            using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher(System.Data.IsolationLevel.ReadCommitted))
            {
                await AcquireExclusiveApprenticeshipUploadLockForProvider(providerId, dispatcher);

                var apprenticeshipUpload = await dispatcher.ExecuteQuery(new GetLatestUnpublishedApprenticeshipUploadForProvider()
                {
                    ProviderId = providerId
                });

                if (apprenticeshipUpload == null)
                {
                    throw new InvalidStateException(InvalidStateReason.NoUnpublishedCourseUpload);
                }

                if (apprenticeshipUpload.UploadStatus != UploadStatus.ProcessedSuccessfully &&
                    apprenticeshipUpload.UploadStatus != UploadStatus.ProcessedWithErrors)
                {
                    throw new InvalidUploadStatusException(
                        apprenticeshipUpload.UploadStatus,
                        UploadStatus.ProcessedSuccessfully,
                        UploadStatus.ProcessedWithErrors);
                }

                // If the world around us has changed (courses added etc.) then we might need to revalidate
                var uploadStatus = await RevalidateApprenticeshipUploadIfRequired(dispatcher, apprenticeshipUpload.ApprenticeshipUploadId);

                var (rows, _) = await dispatcher.ExecuteQuery(new GetApprenticeshipUploadRows()
                {
                    ApprenticeshipUploadId = apprenticeshipUpload.ApprenticeshipUploadId
                });

                await dispatcher.Commit();

                return (rows, uploadStatus);
            }
        }


        public async Task DeleteApprenticeshipUploadForProvider(Guid providerId)
        {
            using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher(System.Data.IsolationLevel.ReadCommitted))
            {
                await AcquireExclusiveCourseUploadLockForProvider(providerId, dispatcher);

                var apprenticeshipUpload = await dispatcher.ExecuteQuery(new GetLatestUnpublishedApprenticeshipUploadForProvider()
                {
                    ProviderId = providerId
                });

                if (apprenticeshipUpload == null)
                {
                    throw new InvalidStateException(InvalidStateReason.NoUnpublishedApprenticeshipUpload);
                }

                if (apprenticeshipUpload.UploadStatus != UploadStatus.ProcessedWithErrors &&
                    apprenticeshipUpload.UploadStatus != UploadStatus.ProcessedSuccessfully)
                {
                    throw new InvalidUploadStatusException(
                        apprenticeshipUpload.UploadStatus,
                        UploadStatus.ProcessedWithErrors,
                        UploadStatus.ProcessedSuccessfully);
                }

                await dispatcher.ExecuteQuery(
                    new SetApprenticeshipUploadAbandoned()
                    {
                        ApprenticeshipUploadId = apprenticeshipUpload.ApprenticeshipUploadId,
                        AbandonedOn = _clock.UtcNow
                    });

                await dispatcher.Commit();
            }
        }

        private async Task<UploadStatus> RefreshApprenticeshipUploadValidationStatus(Guid apprenticeshipUploadId, ISqlQueryDispatcher sqlQueryDispatcher)
        {
            var uploadIsValid = (await sqlQueryDispatcher.ExecuteQuery(new GetApprenticeshipUploadInvalidRowCount() { ApprenticeshipUploadId = apprenticeshipUploadId })) == 0;

            await sqlQueryDispatcher.ExecuteQuery(new SetApprenticeshipUploadProcessed()
            {
                ApprenticeshipUploadId = apprenticeshipUploadId,
                ProcessingCompletedOn = _clock.UtcNow,
                IsValid = uploadIsValid
            });

            return uploadIsValid ? UploadStatus.ProcessedSuccessfully : UploadStatus.ProcessedWithErrors;
        }

        internal Venue FindVenue(ApprenticeshipDataUploadRowInfo row, IReadOnlyCollection<Venue> providerVenues)
        {
            if (row.VenueIdHint.HasValue)
            {
                return providerVenues.Single(v => v.VenueId == row.VenueIdHint);
            }

            if (!string.IsNullOrEmpty(row.Data.YourVenueReference))
            {
                // N.B. Using `Count()` here instead of `Single()` to protect against bad data where we have duplicates

                var matchedVenues = providerVenues
                    .Where(v => RefMatches(row.Data.YourVenueReference, v))
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


        internal class ApprenticeshipUploadRowValidator : AbstractValidator<ParsedCsvApprenticeshipRow>
        {
            public ApprenticeshipUploadRowValidator(
                Guid? matchedVenueId,
                IList<ParsedCsvApprenticeshipRow> allRows)
            {
                RuleFor(c => c.StandardCode).Transform(x => int.TryParse(x, out int standardCode) ? (int?)standardCode : null).StandardCode();
                RuleFor(c => c.StandardVersion).Transform(x => int.TryParse(x, out int standardVersion) ? (int?)standardVersion : null).StandardVersion();
                RuleFor(c => c.ApprenticeshipInformation).MarketingInformation();
                RuleFor(c => c.ApprenticeshipWebpage).Website();
                RuleFor(c => c.ContactEmail).ContactEmail();
                RuleFor(c => c.ContactPhone).ContactTelephone();
                RuleFor(c => c.ContactUrl).ContactWebsite();
                RuleFor(c => c.ResolvedDeliveryModes).DeliveryMode(c => c.ResolvedDeliveryMethod);
                RuleFor(c => c.ResolvedDeliveryMethod).DeliveryMethod();
                RuleFor(c => c.YourVenueReference).YourVenueReference(c => c.ResolvedDeliveryMethod, c => c.VenueName, matchedVenueId);
                RuleFor(c => c.ResolvedRadius).Radius(c => c.ResolvedDeliveryMethod, c => c.ResolvedNationalDelivery);
                RuleFor(c => c.ResolvedNationalDelivery).NationalDelivery(
                    c => c.ResolvedDeliveryMethod,
                    c => c.ResolvedRadius);
                RuleFor(c => c.ResolvedSubRegions).SubRegions(
                    subRegionsWereSpecified: c => !string.IsNullOrEmpty(c.SubRegion),
                    c => c.ResolvedDeliveryMethod,
                    c => c.ResolvedNationalDelivery.HasValue ? c.ResolvedNationalDelivery.Value : false);
                RuleFor(c => c.VenueName).VenueName(c => c.ResolvedDeliveryMethod, c => c.YourVenueReference, matchedVenueId);

                RuleFor(c => c).Custom((row, ctx) =>
                {
                    if (row.ResolvedDeliveryMethod != ApprenticeshipLocationType.EmployerBased)
                    {
                        return;
                    }

                    // Check there's at most one Employer based row for a given standard+version
                    var standardCode = int.Parse(row.StandardCode);
                    var standardVersion = int.Parse(row.StandardVersion);

                    var rowsWithStandard = allRows.Count(r =>
                        r.ResolvedDeliveryMethod == ApprenticeshipLocationType.EmployerBased &&
                        int.Parse(r.StandardCode) == standardCode &&
                        int.Parse(r.StandardVersion) == standardVersion);

                    if (rowsWithStandard > 1)
                    {
                        ctx.AddFailure(
                            ValidationFailureEx.CreateFromErrorCode(ctx.PropertyName, "APPRENTICESHIP_DUPLICATE_STANDARDCODE"));
                    }
                });
            }
        }
    }
}
