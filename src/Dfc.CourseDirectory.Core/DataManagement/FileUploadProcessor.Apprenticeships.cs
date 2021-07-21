using System;
using System.IO;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;

namespace Dfc.CourseDirectory.Core.DataManagement
{
    public partial class FileUploadProcessor
    {
        public async Task<SaveFileResult> SaveApprenticeshipFile(Guid providerId, Stream stream, UserInfo uploadedBy)
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

            var (fileMatchesSchemaResult, missingHeaders) = await FileMatchesSchema<CsvApprenticeshipRow>(stream);
            if (fileMatchesSchemaResult == FileMatchesSchemaResult.InvalidHeader)
            {
                return SaveFileResult.InvalidHeader(missingHeaders);
            }


            var apprenticeshipUploadId = Guid.NewGuid();

            using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher())
            {
                // Check there isn't an existing unprocessed upload for this provider

                var existingUpload = await dispatcher.ExecuteQuery(new GetLatestUnpublishedApprenticeshipUploadForProvider()
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


            async Task UploadToBlobStorage()
            {
                if (!_containerIsKnownToExist)
                {
                    await _blobContainerClient.CreateIfNotExistsAsync();
                    _containerIsKnownToExist = true;
                }

                var blobName = $"{Constants.ApprenticeshipFolder}/{apprenticeshipUploadId}.csv";
                await _blobContainerClient.UploadBlobAsync(blobName, stream);
            }

            return SaveFileResult.Success(apprenticeshipUploadId, UploadStatus.Created);
        }

    }
}
