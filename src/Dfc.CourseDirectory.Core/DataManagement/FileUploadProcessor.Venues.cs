using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using FluentValidation;

namespace Dfc.CourseDirectory.Core.DataManagement
{
    public partial class FileUploadProcessor
    {
        public IObservable<UploadStatus> GetVenueUploadStatusUpdates(Guid venueUploadId) =>
            GetPollingVenueUploadStatusUpdates(venueUploadId)
                .DistinctUntilChanged()
                .TakeUntil(status => status.IsTerminal());

        public async Task ProcessVenueFile(Guid venueUploadId, Stream stream)
        {
            // N.B. Every part of this method needs to be idempotent

            using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher())
            {
                await dispatcher.ExecuteQuery(new SetVenueUploadProcessing()
                {
                    VenueUploadId = venueUploadId,
                    ProcessingStartedOn = _clock.UtcNow
                });

                await dispatcher.Commit();
            }

            // At this point `stream` should be a CSV that's already known to conform to `VenueRow`'s schema.
            // We read all the rows upfront because validation needs to do duplicate checking.
            // We also don't expect massive files here so reading everything into memory is ok.
            List<VenueRow> rows;
            using (var streamReader = new StreamReader(stream))
            using (var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture))
            {
                rows = await csvReader.GetRecordsAsync<VenueRow>().ToListAsync();
            }

            using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher())
            {
                var venueUpload = await dispatcher.ExecuteQuery(new GetVenueUpload() { VenueUploadId = venueUploadId });
                var providerId = venueUpload.ProviderId;

                var isValid = await ValidateAndUpsertVenueRows(dispatcher, venueUploadId, providerId, rows);

                await dispatcher.ExecuteQuery(new SetVenueUploadProcessed()
                {
                    VenueUploadId = venueUploadId,
                    ProcessingCompletedOn = _clock.UtcNow,
                    IsValid = isValid
                });

                await dispatcher.Commit();
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

            var (fileMatchesSchemaResult, missingHeaders) = await FileMatchesSchema<VenueRow>(stream);
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

                var existingUpload = await dispatcher.ExecuteQuery(new GetLatestVenueUploadForProviderWithStatus()
                {
                    ProviderId = providerId,
                    Statuses = new[] { UploadStatus.Created, UploadStatus.Processing }
                });

                if (existingUpload != null)
                {
                    return SaveFileResult.ExistingFileInFlight();
                }

                // Abandon any existing un-published upload (there will be one at most)

                var unpublishedUpload = await dispatcher.ExecuteQuery(new GetLatestVenueUploadForProviderWithStatus()
                {
                    ProviderId = providerId,
                    Statuses = new[] { UploadStatus.ProcessedSuccessfully, UploadStatus.ProcessedWithErrors }
                });

                if (unpublishedUpload != null)
                {
                    await dispatcher.ExecuteQuery(new SetVenueUploadAbandoned()
                    {
                        VenueUploadId = unpublishedUpload.VenueUploadId,
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

        public Task WaitForVenueProcessingToComplete(Guid venueUploadId, CancellationToken cancellationToken) =>
            GetVenueUploadStatusUpdates(venueUploadId)
                .TakeUntil(status => status != UploadStatus.Created && status != UploadStatus.Processing)
                .ForEachAsync(_ => { }, cancellationToken);

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
        protected virtual IObservable<UploadStatus> GetPollingVenueUploadStatusUpdates(Guid venueUploadId) =>
            Observable.Interval(_statusUpdatesPollInterval)
                .SelectMany(_ => Observable.FromAsync(() => GetVenueUploadStatus(venueUploadId)));

        // internal for testing
        internal async Task<bool> ValidateAndUpsertVenueRows(
            ISqlQueryDispatcher sqlQueryDispatcher,
            Guid venueUploadId,
            Guid providerId,
            IReadOnlyList<VenueRow> rows)
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
            rows = rows.Concat(venuesWithLiveOfferingsNotInFile.Select(VenueRow.FromModel)).ToArray();
            rowVenueIdMapping = rowVenueIdMapping.Concat(venuesWithLiveOfferingsNotInFile.Select(v => (Guid?)v.VenueId)).ToArray();

            var uploadIsValid = true;
            var validator = new VenueUploadRowValidator();

            var upsertRecords = new List<UpsertVenueUploadRowsRecord>();

            for (int i = 0; i < rows.Count; i++)
            {
                var row = rows[i];

                // `i` is zero-based and the headers take up one row;
                // we want a row number that aligns with the row number as it appears when viewing the CSV in Excel
                var rowNumber = i + 2;

                var venueId = rowVenueIdMapping[i];
                var isSupplementaryRow = i >= originalRowCount;

                var rowValidatonResult = validator.Validate(row);
                var errors = rowValidatonResult.Errors.Select(e => e.ErrorCode).ToArray();
                var rowIsValid = rowValidatonResult.IsValid;
                uploadIsValid &= rowIsValid;

                upsertRecords.Add(new UpsertVenueUploadRowsRecord()
                {
                    RowNumber = rowNumber,
                    IsValid = rowIsValid,
                    Errors = errors,
                    IsSupplementary = isSupplementaryRow,
                    VenueId = venueId,
                    ProviderVenueRef = row.ProviderVenueRef,
                    VenueName = row.VenueName,
                    AddressLine1 = row.AddressLine1,
                    AddressLine2 = row.AddressLine2,
                    Town = row.Town,
                    County = row.County,
                    Postcode = Postcode.TryParse(row.Postcode, out var postcode) ? postcode : row.Postcode,
                    Email = row.Email,
                    Telephone = row.Telephone,
                    Website = row.Website
                });
            }

            await sqlQueryDispatcher.ExecuteQuery(new UpsertVenueUploadRows()
            {
                VenueUploadId = venueUploadId,
                CreatedOn = _clock.UtcNow,
                Records = upsertRecords
            });

            return uploadIsValid;
        }

        // internal for testing
        internal Guid?[] MatchRowsToExistingVenues(IReadOnlyList<VenueRow> rows, IEnumerable<Venue> existingVenues)
        {
            var rowVenueIdMapping = new Guid?[rows.Count];

            var remainingCandidates = existingVenues.ToList();

            // First try to match on ProviderVenueRef..
            MatchOnPredicate((row, venue) => !string.IsNullOrWhiteSpace(row.ProviderVenueRef) &&
                row.ProviderVenueRef.Equals(venue.ProviderVenueRef, StringComparison.OrdinalIgnoreCase));

            // ..then on VenueName
            MatchOnPredicate((row, venue) => row.VenueName.Equals(venue.VenueName, StringComparison.OrdinalIgnoreCase));

            return rowVenueIdMapping;

            void MatchOnPredicate(Func<VenueRow, Venue, bool> matches)
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
                        if (matches(row, candidate))
                        {
                            remainingCandidates.Remove(candidate);
                            rowVenueIdMapping[i] = candidate.VenueId;
                            continue;
                        }
                    }
                }
            }
        }

        private class VenueUploadRowValidator : AbstractValidator<VenueRow>
        { }
    }
}
