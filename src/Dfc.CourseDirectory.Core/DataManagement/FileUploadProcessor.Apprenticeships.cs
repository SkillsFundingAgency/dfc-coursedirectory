﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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

                var blobName = $"{Constants.ApprenticeshipsFolder}/{apprenticeshipUploadId}.csv";
                await _blobContainerClient.UploadBlobAsync(blobName, stream);
            }

            return SaveFileResult.Success(apprenticeshipUploadId, UploadStatus.Created);
        }

        public async Task ProcessApprenticeshipFile(Guid apprenticeshipUploadId, Stream stream)
        {
            using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher())
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
            using (var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture))
            {
                rows = await csvReader.GetRecordsAsync<CsvApprenticeshipRow>().ToListAsync();
            }

            var rowsCollection = CreateApprenticeshipDataUploadRowInfoCollection();

            using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher())
            {
                var venueUpload = await dispatcher.ExecuteQuery(new GetApprenticeshipUpload() { ApprenticeshipUploadId = apprenticeshipUploadId });
                var providerId = venueUpload.ProviderId;

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
                var apprenticeshipId = Guid.NewGuid();
                var rowInfos = new List<ApprenticeshipDataUploadRowInfo>();
                foreach (var row in rows)
                {
                    rowInfos.Add(new ApprenticeshipDataUploadRowInfo(row, rowNumber: rowInfos.Count + 2, apprenticeshipId));
                }
                return new ApprenticeshipDataUploadRowInfoCollection(rowInfos);
            }
        }

        // internal for testing
        internal async Task<(UploadStatus uploadStatus, IReadOnlyCollection<ApprenticeshipUploadRow> Rows)> ValidateApprenticeshipUploadRows(
            ISqlQueryDispatcher sqlQueryDispatcher,
            Guid apprenticeshipUploadId,
            Guid providerId,
            ApprenticeshipDataUploadRowInfoCollection rows)
        {
            var providerVenues = await sqlQueryDispatcher.ExecuteQuery(new GetVenuesByProvider() { ProviderId = providerId });

            var rowsAreValid = true;

            var upsertRecords = new List<UpsertApprenticeshipUploadRowsRecord>();

            foreach (var row in rows)
            {
                var rowNumber = row.RowNumber;;
                var parsedRow = ParsedCsvApprenticeshipRow.FromCsvApprenticeshipRow(row.Data);
                var validator = new ApprenticeshipUploadRowValidator(_clock);
                var rowValidationResult = validator.Validate(parsedRow);
                var errors = rowValidationResult.Errors.Select(e => e.ErrorCode).ToArray();
                var rowIsValid = rowValidationResult.IsValid;
                rowsAreValid &= rowIsValid;

                int.TryParse(parsedRow.StandardCode, out int standardCode);
                int.TryParse(parsedRow.StandardVersion, out int standardVersion);

                upsertRecords.Add(new UpsertApprenticeshipUploadRowsRecord()
                {
                    RowNumber = rowNumber,
                    IsValid = rowIsValid,
                    Errors = errors,
                    ApprenticeshipId = row.ApprenticeshipId,
                    StandardCode = standardCode,
                    StandardVersion = standardVersion,
                    ApprenticeshipInformation = parsedRow.ApprenticeshipInformation,
                    ApprenticeshipWebpage = parsedRow.ApprenticeshipWebpage,
                    ContactEmail = parsedRow.ContactEmail,
                    ContactPhone = parsedRow.ContactPhone,
                    ContactUrl = parsedRow.ContactUrl,
                    DeliveryMethod = parsedRow.DeliveryMethod,
                    Venue = parsedRow.Venue,
                    YourVenueReference = parsedRow.YourVenueReference,
                    Radius = parsedRow.Radius,
                    DeliveryMode = parsedRow.DeliveryMode,
                    NationalDelivery = parsedRow.NationalDelivery,
                    SubRegions = parsedRow.SubRegion
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

            var uploadStatus = rowsAreValid ? UploadStatus.ProcessedSuccessfully : UploadStatus.ProcessedWithErrors;

            return (uploadStatus, updatedRows);
        }

        internal class ApprenticeshipUploadRowValidator : AbstractValidator<ParsedCsvApprenticeshipRow>
        {
            public ApprenticeshipUploadRowValidator(
                IClock clock)
            {
            }
        }
    }
}
