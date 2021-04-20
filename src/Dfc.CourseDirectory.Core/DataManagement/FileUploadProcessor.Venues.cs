using System;
using System.IO;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataManagement
{
    public partial class FileUploadProcessor
    {
        public IObservable<UploadStatus> GetVenueUploadStatusUpdates(Guid venueUploadId) =>
            Observable.Create<UploadStatus>(async (observer, cancellationToken) =>
            {
                // The IsolationLevel override here is important - our default Snapshot would never see data changes
                // since since they happen in other transactions.
                using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher(System.Data.IsolationLevel.ReadCommitted))
                {
                    while (true)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var venueUpload = await dispatcher.ExecuteQuery(new GetVenueUpload() { VenueUploadId = venueUploadId });

                        if (venueUpload == null)
                        {
                            observer.OnError(new ArgumentException("Specified venue upload does not exist.", nameof(venueUploadId)));
                            return;
                        }

                        observer.OnNext(venueUpload.UploadStatus);

                        if (venueUpload.UploadStatus.IsTerminal())
                        {
                            observer.OnCompleted();
                            return;
                        }

                        await Task.Delay(_pollInterval, cancellationToken);
                    }
                }
            }).DistinctUntilChanged();

        public async Task ProcessVenueFile(Guid venueUploadId, Stream stream)
        {
            using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher())
            {
                await dispatcher.ExecuteQuery(new UpdateVenueUploadStatus()
                {
                    ChangedOn = _clock.UtcNow,
                    UploadStatus = UploadStatus.InProgress,
                    VenueUploadId = venueUploadId
                });

                await dispatcher.Transaction.CommitAsync();
            }

            // TODO Actually process the file
            // TEMP status changes with delay for testing
            await Task.Delay(new Random().Next(0, 10000));

            using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher())
            {
                await dispatcher.ExecuteQuery(new UpdateVenueUploadStatus()
                {
                    ChangedOn = _clock.UtcNow,
                    UploadStatus = UploadStatus.Processed,
                    VenueUploadId = venueUploadId
                });

                await dispatcher.Transaction.CommitAsync();
            }
        }

        public async Task<SaveFileResult> SaveVenueFile(Guid providerId, Stream stream, UserInfo uploadedBy)
        {
            if (!stream.CanRead)
            {
                throw new ArgumentException("Stream must be readable.", nameof(stream));
            }

            if (!stream.CanSeek)
            {
                throw new ArgumentException("Stream must be seekable.", nameof(stream));
            }

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
                    await dispatcher.ExecuteQuery(new UpdateVenueUploadStatus()
                    {
                        ChangedOn = _clock.UtcNow,
                        UploadStatus = UploadStatus.Abandoned,
                        VenueUploadId = unpublishedUpload.VenueUploadId
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
    }
}
