using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Validation.VenueValidation;
using FluentValidation;

namespace Dfc.CourseDirectory.Core.DataManagement
{
    public partial class FileUploadProcessor
    {
        public async Task DeleteVenueUploadForProvider(Guid providerId)
        {
            using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher())
            {
                var venueUpload = await dispatcher.ExecuteQuery(new GetLatestUnpublishedVenueUploadForProvider()
                {
                    ProviderId = providerId
                });

                if (venueUpload == null)
                {
                    throw new InvalidStateException(InvalidStateReason.NoUnpublishedVenueUpload);
                }

                if (venueUpload.UploadStatus != UploadStatus.ProcessedWithErrors &&
                    venueUpload.UploadStatus != UploadStatus.ProcessedSuccessfully)
                {
                    throw new InvalidUploadStatusException(
                        venueUpload.UploadStatus,
                        UploadStatus.ProcessedWithErrors,
                        UploadStatus.ProcessedSuccessfully);
                }

                await dispatcher.ExecuteQuery(
                    new SetVenueUploadAbandoned()
                    {
                        VenueUploadId = venueUpload.VenueUploadId,
                        AbandonedOn = _clock.UtcNow
                    });

                await dispatcher.Commit();
            }
        }

        public async Task<(IReadOnlyCollection<VenueUploadRow> Rows, UploadStatus UploadStatus)> GetVenueUploadRowsForProvider(Guid providerId)
        {
            using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher())
            {
                var venueUpload = await dispatcher.ExecuteQuery(new GetLatestUnpublishedVenueUploadForProvider()
                {
                    ProviderId = providerId
                });

                if (venueUpload == null)
                {
                    throw new InvalidStateException(InvalidStateReason.NoUnpublishedVenueUpload);
                }

                if (venueUpload.UploadStatus != UploadStatus.ProcessedSuccessfully &&
                    venueUpload.UploadStatus != UploadStatus.ProcessedWithErrors)
                {
                    throw new InvalidUploadStatusException(
                        venueUpload.UploadStatus,
                        UploadStatus.ProcessedSuccessfully,
                        UploadStatus.ProcessedWithErrors);
                }

                // If the world around us has changed (courses added etc.) then we might need to revalidate
                var (_, _, rows) = await RevalidateVenueUploadIfRequired(dispatcher, venueUpload.VenueUploadId);

                // rows will only be non-null if revalidation was done above
                rows ??= (await dispatcher.ExecuteQuery(new GetVenueUploadRows() { VenueUploadId = venueUpload.VenueUploadId })).Rows;

                await dispatcher.Commit();

                return (rows, venueUpload.UploadStatus);
            }
        }

        public IObservable<UploadStatus> GetVenueUploadStatusUpdatesForProvider(Guid providerId)
        {
            return GetVenueUploadId().ToObservable()
                .SelectMany(venueUploadId => GetVenueUploadStatusUpdates(venueUploadId))
                .DistinctUntilChanged()
                .TakeUntil(status => status.IsTerminal());

            async Task<Guid> GetVenueUploadId()
            {
                using var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher();
                var venueUpload = await dispatcher.ExecuteQuery(new GetLatestUnpublishedVenueUploadForProvider() { ProviderId = providerId });

                if (venueUpload == null)
                {
                    throw new InvalidStateException(InvalidStateReason.NoUnpublishedVenueUpload);
                }

                return venueUpload.VenueUploadId;
            }
        }

        public async Task ProcessVenueFile(Guid venueUploadId, Stream stream)
        {
            using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher())
            {
                var setProcessingResult = await dispatcher.ExecuteQuery(new SetVenueUploadProcessing()
                {
                    VenueUploadId = venueUploadId,
                    ProcessingStartedOn = _clock.UtcNow
                });

                if (setProcessingResult != SetVenueUploadProcessingResult.Success)
                {
                    await DeleteBlob();

                    return;
                }

                await dispatcher.Commit();
            }

            // At this point `stream` should be a CSV that's already known to conform to `CsvVenueRow`'s schema.
            // We read all the rows upfront because validation needs to do duplicate checking.
            // We also don't expect massive files here so reading everything into memory is ok.
            List<CsvVenueRow> rows;
            using (var streamReader = new StreamReader(stream))
            using (var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture))
            {
                rows = await csvReader.GetRecordsAsync<CsvVenueRow>().ToListAsync();
            }

            var rowsCollection = new VenueDataUploadRowInfoCollection(
                lastRowNumber: rows.Count + 1,
                rows: rows.Select((r, i) => new VenueDataUploadRowInfo(r, rowNumber: i + 2, isSupplementary: false)));

            using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher())
            {
                var venueUpload = await dispatcher.ExecuteQuery(new GetVenueUpload() { VenueUploadId = venueUploadId });
                var providerId = venueUpload.ProviderId;

                await ValidateVenueUploadFile(dispatcher, venueUploadId, providerId, rowsCollection);

                await dispatcher.Commit();
            }

            await DeleteBlob();

            Task DeleteBlob()
            {
                var blobName = $"{Constants.VenuesFolder}/{venueUploadId}.csv";
                return _blobContainerClient.DeleteBlobIfExistsAsync(blobName);
            }
        }

        public async Task<PublishResult> PublishVenueUploadForProvider(Guid providerId, UserInfo publishedBy)
        {
            using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher())
            {
                var venueUpload = await dispatcher.ExecuteQuery(new GetLatestUnpublishedVenueUploadForProvider()
                {
                    ProviderId = providerId
                });

                if (venueUpload == null)
                {
                    throw new InvalidStateException(InvalidStateReason.NoUnpublishedVenueUpload);
                }

                if (venueUpload.UploadStatus.IsUnprocessed())
                {
                    throw new InvalidUploadStatusException(
                        venueUpload.UploadStatus,
                        UploadStatus.ProcessedWithErrors,
                        UploadStatus.ProcessedSuccessfully);
                }

                if (venueUpload.UploadStatus == UploadStatus.ProcessedWithErrors)
                {
                    return PublishResult.UploadHasErrors();
                }

                var (revalidated, _, rows) = await RevalidateVenueUploadIfRequired(dispatcher, venueUpload.VenueUploadId);

                if (revalidated && rows.Any(r => !r.IsValid))
                {
                    return PublishResult.UploadHasErrors();
                }

                var publishedOn = _clock.UtcNow;

                var publishResult = await dispatcher.ExecuteQuery(new PublishVenueUpload()
                {
                    VenueUploadId = venueUpload.VenueUploadId,
                    PublishedBy = publishedBy,
                    PublishedOn = publishedOn
                });

                await dispatcher.Commit();

                return PublishResult.Success(publishResult.AsT1.PublishedCount);
            }
        }

        public async Task<SaveFileResult> SaveVenueFile(Guid providerId, Stream stream, UserInfo uploadedBy)
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

            var (fileMatchesSchemaResult, missingHeaders) = await FileMatchesSchema<CsvVenueRow>(stream);
            if (fileMatchesSchemaResult == FileMatchesSchemaResult.InvalidHeader)
            {
                return SaveFileResult.InvalidHeader(missingHeaders);
            }
            else if (fileMatchesSchemaResult == FileMatchesSchemaResult.InvalidRows)
            {
                return SaveFileResult.InvalidRows();
            }

            var venueUploadId = Guid.NewGuid();

            using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher())
            {
                // Check there isn't an existing unprocessed upload for this provider

                var existingUpload = await dispatcher.ExecuteQuery(new GetLatestUnpublishedVenueUploadForProvider()
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
                    await dispatcher.ExecuteQuery(new SetVenueUploadAbandoned()
                    {
                        VenueUploadId = existingUpload.VenueUploadId,
                        AbandonedOn = _clock.UtcNow
                    });
                }

                await dispatcher.ExecuteQuery(new CreateVenueUpload()
                {
                    CreatedBy = uploadedBy,
                    CreatedOn = _clock.UtcNow,
                    ProviderId = providerId,
                    VenueUploadId = venueUploadId
                });

                await dispatcher.Transaction.CommitAsync();
            }

            await UploadToBlobStorage();

            return SaveFileResult.Success(venueUploadId, UploadStatus.Created);

            async Task UploadToBlobStorage()
            {
                if (!_containerIsKnownToExist)
                {
                    await _blobContainerClient.CreateIfNotExistsAsync();
                    _containerIsKnownToExist = true;
                }

                var blobName = $"{Constants.VenuesFolder}/{venueUploadId}.csv";
                await _blobContainerClient.UploadBlobAsync(blobName, stream);
            }
        }

        public async Task<UploadStatus> UpdateVenueUploadRowForProvider(Guid providerId, int rowNumber, CsvVenueRow updatedRow)
        {
            using var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher();

            var venueUpload = await dispatcher.ExecuteQuery(new GetLatestUnpublishedVenueUploadForProvider()
            {
                ProviderId = providerId
            });

            if (venueUpload == null)
            {
                throw new InvalidStateException(InvalidStateReason.NoUnpublishedVenueUpload);
            }

            if (venueUpload.UploadStatus != UploadStatus.ProcessedWithErrors)
            {
                throw new InvalidUploadStatusException(venueUpload.UploadStatus, UploadStatus.ProcessedWithErrors);
            }

            var (rows, lastRowNumber) = await dispatcher.ExecuteQuery(new GetVenueUploadRows() { VenueUploadId = venueUpload.VenueUploadId });

            var row = rows.SingleOrDefault(r => r.RowNumber == rowNumber);

            if (row == null)
            {
                throw new ResourceDoesNotExistException(ResourceType.VenueUploadRow, rowNumber);
            }

            var updatedRows = new VenueDataUploadRowInfoCollection(
                lastRowNumber: lastRowNumber,
                rows
                    .Where(r => r.RowNumber != rowNumber)
                    .Select(r => new VenueDataUploadRowInfo(CsvVenueRow.FromModel(r), r.RowNumber, r.IsSupplementary))
                    .Append(new VenueDataUploadRowInfo(updatedRow, rowNumber, row.IsSupplementary)));

            var (uploadStatus, _) = await ValidateVenueUploadFile(dispatcher, venueUpload.VenueUploadId, venueUpload.ProviderId, updatedRows);

            await dispatcher.Commit();

            return uploadStatus;
        }

        public Task<UploadStatus> WaitForVenueProcessingToCompleteForProvider(Guid providerId, CancellationToken cancellationToken) =>
            GetVenueUploadStatusUpdatesForProvider(providerId)
                .TakeUntil(status => status != UploadStatus.Created && status != UploadStatus.Processing)
                .LastAsync()
                .ToTask(cancellationToken);

        protected async Task<UploadStatus> GetVenueUploadStatus(Guid venueUploadId)
        {
            using var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher();
            var venueUpload = await dispatcher.ExecuteQuery(new GetVenueUpload() { VenueUploadId = venueUploadId });

            if (venueUpload == null)
            {
                throw new ArgumentException("Specified venue upload does not exist.", nameof(venueUploadId));
            }

            return venueUpload.UploadStatus;
        }

        // virtual for testing
        protected virtual IObservable<UploadStatus> GetVenueUploadStatusUpdates(Guid venueUploadId) =>
            Observable.Interval(_statusUpdatesPollInterval)
                .SelectMany(_ => Observable.FromAsync(() => GetVenueUploadStatus(venueUploadId)));

        // internal for testing
        internal async Task<(UploadStatus uploadStatus, IReadOnlyCollection<VenueUploadRow> Rows)> ValidateVenueUploadFile(
            ISqlQueryDispatcher sqlQueryDispatcher,
            Guid venueUploadId,
            Guid providerId,
            VenueDataUploadRowInfoCollection rows)
        {
            // We need to ensure that any venues that have live offerings attached are not removed when publishing this
            // upload. We do that by adding an additional row to this upload for any venues that are not included in
            // this file that have live offerings attached. This must be done *before* validation so that duplicate
            // checks consider these additional added rows.

            var originalRowCount = rows.Count;

            var existingVenues = await sqlQueryDispatcher.ExecuteQuery(new GetVenueMatchInfoForProvider() { ProviderId = providerId });

            // For each row in the file try to match it to an existing venue
            var rowVenueIdMapping = MatchRowsToExistingVenues(rows, existingVenues);

            // Add a row for any existing venues that are linked to live offerings and haven't been matched
            var matchedVenueIds = rowVenueIdMapping.Where(m => m.HasValue).Select(m => m.Value).ToArray();
            var venuesWithLiveOfferingsNotInFile = existingVenues
                .Where(v => v.HasLiveOfferings && !matchedVenueIds.Contains(v.VenueId))
                .ToArray();

            rows = new VenueDataUploadRowInfoCollection(
                lastRowNumber: rows.LastRowNumber + venuesWithLiveOfferingsNotInFile.Length,
                rows: rows.Concat(
                    venuesWithLiveOfferingsNotInFile.Select((v, i) =>
                        new VenueDataUploadRowInfo(CsvVenueRow.FromModel(v), rowNumber: rows.LastRowNumber + i + 1, isSupplementary: true))));

            rowVenueIdMapping = rowVenueIdMapping.Concat(venuesWithLiveOfferingsNotInFile.Select(v => (Guid?)v.VenueId)).ToArray();

            // Grab PostcodeInfo for all of the valid postcodes in the file.
            // We need this for both the validator and to track whether the venue is outside of England
            var allPostcodeInfo = await GetPostcodeInfoForRows(sqlQueryDispatcher, rows);

            var uploadIsValid = true;
            var validator = new VenueUploadRowValidator(rows, allPostcodeInfo);

            var upsertRecords = new List<SetVenueUploadRowsRecord>();

            for (int i = 0; i < rows.Count; i++)
            {
                var row = rows[i].Data;
                var rowNumber = rows[i].RowNumber;

                var venueId = rowVenueIdMapping[i] ?? Guid.NewGuid();
                var isSupplementaryRow = i >= originalRowCount;

                // A row is deletable if it is *not* matched to an existing venue that has attached offerings
                var isDeletable = rowVenueIdMapping[i] is null ||
                    !existingVenues.Single(v => v.VenueId == rowVenueIdMapping[i]).HasLiveOfferings;

                row.ProviderVenueRef = row.ProviderVenueRef?.Trim();
                row.Postcode = Postcode.TryParse(row.Postcode, out var postcode) ? postcode : row.Postcode;

                PostcodeInfo postcodeInfo = null;
                if (postcode != null)
                {
                    allPostcodeInfo.TryGetValue(postcode, out postcodeInfo);
                }

                var rowValidationResult = validator.Validate(row);
                var errors = rowValidationResult.Errors.Select(e => e.ErrorCode).ToArray();
                var rowIsValid = rowValidationResult.IsValid;
                uploadIsValid &= rowIsValid;

                upsertRecords.Add(new SetVenueUploadRowsRecord()
                {
                    RowNumber = rowNumber,
                    IsValid = rowIsValid,
                    Errors = errors,
                    IsSupplementary = isSupplementaryRow,
                    OutsideOfEngland = postcodeInfo != null ? !postcodeInfo.InEngland : (bool?)null,
                    VenueId = venueId,
                    IsDeletable = isDeletable,
                    ProviderVenueRef = row.ProviderVenueRef,
                    VenueName = row.VenueName,
                    AddressLine1 = row.AddressLine1,
                    AddressLine2 = row.AddressLine2,
                    Town = row.Town,
                    County = row.County,
                    Postcode = row.Postcode,
                    Email = row.Email,
                    Telephone = row.Telephone,
                    Website = row.Website
                });
            }

            var updatedRows = await sqlQueryDispatcher.ExecuteQuery(new SetVenueUploadRows()
            {
                VenueUploadId = venueUploadId,
                ValidatedOn = _clock.UtcNow,
                Records = upsertRecords
            });

            await sqlQueryDispatcher.ExecuteQuery(new SetVenueUploadProcessed()
            {
                VenueUploadId = venueUploadId,
                ProcessingCompletedOn = _clock.UtcNow,
                IsValid = uploadIsValid
            });

            var uploadStatus = uploadIsValid ? UploadStatus.ProcessedSuccessfully : UploadStatus.ProcessedWithErrors;

            return (uploadStatus, updatedRows);
        }

        // internal for testing
        internal Guid?[] MatchRowsToExistingVenues(VenueDataUploadRowInfoCollection rows, IEnumerable<Venue> existingVenues)
        {
            var rowVenueIdMapping = new Guid?[rows.Count];

            var remainingCandidates = existingVenues.ToList();

            // First try to match on ProviderVenueRef..
            MatchOnPredicate((row, venue) =>
                row.ProviderVenueRef?.Equals(venue.ProviderVenueRef, StringComparison.OrdinalIgnoreCase) == true);

            // ..then on VenueName, only where the existing venue does not have a ref
            MatchOnPredicate((row, venue) =>
                string.IsNullOrEmpty(venue.ProviderVenueRef) &&
                    row.VenueName?.Equals(venue.VenueName, StringComparison.OrdinalIgnoreCase) == true);

            return rowVenueIdMapping;

            void MatchOnPredicate(Func<CsvVenueRow, Venue, bool> matches)
            {
                for (int i = 0; i < rows.Count; i++)
                {
                    var row = rows[i];

                    if (rowVenueIdMapping[i].HasValue)
                    {
                        // Already got a match
                        continue;
                    }

                    foreach (var candidate in remainingCandidates.ToArray())
                    {
                        if (matches(row.Data, candidate))
                        {
                            remainingCandidates.Remove(candidate);
                            rowVenueIdMapping[i] = candidate.VenueId;
                            continue;
                        }
                    }
                }
            }
        }

        private async Task<bool> DoesVenueUploadRequireRevalidating(ISqlQueryDispatcher sqlQueryDispatcher, VenueUpload venueUpload)
        {
            if (venueUpload.UploadStatus != UploadStatus.ProcessedWithErrors &&
                venueUpload.UploadStatus != UploadStatus.ProcessedSuccessfully)
            {
                throw new InvalidOperationException($"Venue upload at status {venueUpload.UploadStatus} cannot be revalidated.");
            }

            // Need to revalidate if any venues had been added/updated/removed for this provider
            // (since it affects duplicate checking)
            // or if any courses, apprenticeships or T Levels have had any venues associated or deassociated
            // (since it affects the supplementary rows required).
            // Figuring out these scenarios exactly isn't practical so we check if any
            // venue, course, apprenticeship or T Level has been added/updated/removed for the provider.

            var lastUpdatedOffering = await sqlQueryDispatcher.ExecuteQuery(new GetProviderVenuesLastUpdated()
            {
                ProviderId = venueUpload.ProviderId
            });

            return lastUpdatedOffering.HasValue && lastUpdatedOffering >= venueUpload.LastValidated;
        }

        private async Task<IDictionary<Postcode, PostcodeInfo>> GetPostcodeInfoForRows(
            ISqlQueryDispatcher sqlQueryDispatcher,
            VenueDataUploadRowInfoCollection rows)
        {
            var validPostcodes = rows
                .Select(r => Postcode.TryParse(r.Data.Postcode, out var postcode) ? postcode.ToString() : null)
                .Where(pc => pc != null)
                .Distinct();

            var postcodeInfo = await sqlQueryDispatcher.ExecuteQuery(
                new GetPostcodeInfos() { Postcodes = validPostcodes });

            return postcodeInfo.ToDictionary(kvp => new Postcode(kvp.Key), kvp => kvp.Value);
        }

        private async Task<(bool Revalidated, UploadStatus UploadStatus, IReadOnlyCollection<VenueUploadRow> ValidatedRows)> RevalidateVenueUploadIfRequired(
            ISqlQueryDispatcher sqlQueryDispatcher,
            Guid venueUploadId)
        {
            var venueUpload = await sqlQueryDispatcher.ExecuteQuery(new GetVenueUpload() { VenueUploadId = venueUploadId });

            if (venueUpload == null)
            {
                throw new ArgumentException("Venue upload does not exist.", nameof(venueUploadId));
            }

            var revalidate = await DoesVenueUploadRequireRevalidating(sqlQueryDispatcher, venueUpload);

            if (!revalidate)
            {
                return (Revalidated: false, venueUpload.UploadStatus, ValidatedRows: null);
            }

            var (rows, lastRowNumber) = await sqlQueryDispatcher.ExecuteQuery(new GetVenueUploadRows() { VenueUploadId = venueUploadId });

            var rowCollection = new VenueDataUploadRowInfoCollection(
                lastRowNumber,
                rows.Select(r => new VenueDataUploadRowInfo(CsvVenueRow.FromModel(r), r.RowNumber, r.IsSupplementary)));

            var (uploadStatus, revalidatedRows) = await ValidateVenueUploadFile(
                sqlQueryDispatcher,
                venueUploadId,
                venueUpload.ProviderId,
                rowCollection);

            return (Revalidated: true, uploadStatus, ValidatedRows: revalidatedRows);
        }

        private class VenueUploadRowValidator : AbstractValidator<CsvVenueRow>
        {
            private readonly VenueDataUploadRowInfoCollection _allRows;
            private readonly IDictionary<Postcode, PostcodeInfo> _postcodeInfo;

            public VenueUploadRowValidator(VenueDataUploadRowInfoCollection allRows, IDictionary<Postcode, PostcodeInfo> postcodeInfo)
            {
                _allRows = allRows;
                _postcodeInfo = postcodeInfo;

                // N.B. The rule order here is important; we want errors to be emitted in the same order as the columns
                // appear in the file.

                RuleFor(r => r.ProviderVenueRef)
                    .ProviderVenueRef(getOtherVenueProviderVenueRefs:
                        row => Task.FromResult(_allRows
                            .Select(r => r.Data)
                            .Where(r => r != row && !string.IsNullOrEmpty(r.ProviderVenueRef))
                            .Select(r => r.ProviderVenueRef)));

                RuleFor(r => r.VenueName)
                    .VenueName(getOtherVenueNames:
                        row => Task.FromResult(_allRows
                            .Select(r => r.Data)
                            .Where(r => r != row)
                            .Select(r => r.VenueName)));

                RuleFor(r => r.AddressLine1).AddressLine1();
                RuleFor(r => r.AddressLine2).AddressLine2();
                RuleFor(r => r.Town).Town();
                RuleFor(r => r.County).County();
                RuleFor(r => r.Postcode).Postcode(postcode => _postcodeInfo.TryGetValue(postcode, out var pc) ? pc : null);
                RuleFor(r => r.Email).Email();
                RuleFor(r => r.Telephone).PhoneNumber();
                RuleFor(r => r.Website).Website();
            }
        }
    }
}
