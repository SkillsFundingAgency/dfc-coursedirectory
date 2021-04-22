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
                await dispatcher.ExecuteQuery(new SetVenueUploadInProgress()
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

            var uploadIsValid = true;
            var validator = new VenueUploadRowValidator();

            using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher())
            {
                await dispatcher.ExecuteQuery(new UpsertVenueUploadRows()
                {
                    VenueUploadId = venueUploadId,
                    CreatedOn = _clock.UtcNow,
                    Records = rows.Select((row, i) =>
                    {
                        // `i` is zero-based and the headers take up one row;
                        // we want a row number that aligns with the row number as it appears when viewing the CSV in Excel
                        var rowNumber = i + 2;

                        var rowIsValid = validator.Validate(row).IsValid;

                        uploadIsValid &= rowIsValid;

                        return new UpsertVenueUploadRowsRecord()
                        {
                            RowNumber = rowNumber,
                            IsValid = rowIsValid,
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
                        };
                    })
                });
                
                await dispatcher.ExecuteQuery(new SetVenueUploadProcessed()
                {
                    VenueUploadId = venueUploadId,
                    ProcessingCompletedOn = _clock.UtcNow,
                    IsValid = uploadIsValid
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

            var venueUploadId = Guid.NewGuid();

            using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher())
            {
                // Check there isn't an existing unprocessed upload for this provider

                var existingUpload = await dispatcher.ExecuteQuery(new GetLatestVenueUploadForProviderWithStatus()
                {
                    ProviderId = providerId,
                    Statuses = new[] { UploadStatus.Created, UploadStatus.InProgress }
                });

                if (existingUpload != null)
                {
                    return SaveFileResult.ExistingFileInFlight();
                }

                // Abandon any existing un-published upload (there will be one at most)

                var unpublishedUpload = await dispatcher.ExecuteQuery(new GetLatestVenueUploadForProviderWithStatus()
                {
                    ProviderId = providerId,
                    Statuses = new[] { UploadStatus.Processed }
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
                .TakeUntil(status => status == UploadStatus.Processed || status.IsTerminal())
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

        private class VenueUploadRowValidator : AbstractValidator<VenueRow>
        { }
    }
}
