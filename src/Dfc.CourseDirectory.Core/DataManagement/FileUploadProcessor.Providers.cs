using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Helpers;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Services;
using Dfc.CourseDirectory.Core.Validation.CourseValidation;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataManagement
{
    public partial class FileUploadProcessor
    {
        public async Task ProcessProviderFile(Guid providerUploadId, Stream stream)
        {
            using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher(System.Data.IsolationLevel.ReadCommitted))
            {
                var uploadedCourse = await dispatcher.ExecuteQuery(new GetProviderUpload() { ProviderUploadId = providerUploadId });

                var setProcessingResult = await dispatcher.ExecuteQuery(new SetProviderUploadProcessing()
                {
                    ProviderUploadId = providerUploadId,
                    ProcessingStartedOn = _clock.UtcNow
                });

                if (setProcessingResult != SetProviderUploadProcessingResult.Success)
                {
                    await DeleteBlob();

                    return;
                }

                await dispatcher.Commit();
            }
                List<CsvProviderRow> rows;
                using (var streamReader = new StreamReader(stream))
                using (var csvReader = CreateCsvReader(streamReader))
                {
                    rows = await csvReader.GetRecordsAsync<CsvProviderRow>().ToListAsync();
                }
                var grouped = CsvProviderRow.GroupRows(rows);
                var groupProvidersIds = grouped.Select(g => (ProviderId: Guid.NewGuid(), Rows: g)).ToArray();

                var rowInfos = new List<ProviderDataUploadRowInfo>(rows.Count);

                foreach (var row in rows)
                {
                    var providerId = groupProvidersIds.Single(g => g.Rows.Contains(row)).ProviderId;
                    

                    rowInfos.Add(new ProviderDataUploadRowInfo(row, rowNumber: rowInfos.Count + 2, providerId));
                }

                var rowsCollection = new ProviderDataUploadRowInfoCollection(rowInfos);

                using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher(System.Data.IsolationLevel.ReadCommitted))
                {

                  //  var venueUpload = await dispatcher.ExecuteQuery(new GetCourseUpload() { CourseUploadId = courseUploadId });
                    //var providerId = venueUpload.ProviderId;

                    //await AcquireExclusiveCourseUploadLockForProvider(providerId, dispatcher);

                    await ValidateProviderUploadRows(dispatcher, providerUploadId, rowsCollection);

                    await dispatcher.Commit();
                }

            await DeleteBlob();

            Task DeleteBlob()
            {
                var blobName = $"{Constants.ProvidersFolder}/{providerUploadId}.csv";
                return _blobContainerClient.DeleteBlobIfExistsAsync(blobName);
            }

            
        }

        public async Task<OnboardResult> OnboardProviderUpload(Guid providerUploadId, UserInfo onBoardedBy)
        {
            using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher(System.Data.IsolationLevel.ReadCommitted))
            {

                var providerUpload = await dispatcher.ExecuteQuery(new GetProviderUpload()
                {
                    ProviderUploadId = providerUploadId,
                });

                if (providerUpload == null)
                {
                    throw new InvalidStateException(InvalidStateReason.ProviderUploadNotFound);
                }

                if (providerUpload.UploadStatus.IsUnprocessed())
                {
                    throw new InvalidUploadStatusException(
                        providerUpload.UploadStatus,
                        UploadStatus.ProcessedWithErrors,
                        UploadStatus.ProcessedSuccessfully);
                }

                if (providerUpload.UploadStatus == UploadStatus.ProcessedWithErrors)
                {
                    return OnboardResult.UploadHasErrors();
                }


                var onboardedOn = _clock.UtcNow;

                var onboardProviderResult = await dispatcher.ExecuteQuery(new OnboardProvidedUpload()
                {
                    ProviderUploadId = providerUpload.ProviderUploadId,
                    OnboardedBy = onBoardedBy,
                    OnboardedOn = onboardedOn
                });

                await dispatcher.Commit();

                var onboardedProviderIds = onboardProviderResult.AsT1.OnboardedProviderIds;

                return OnboardResult.Success(onboardedProviderIds.Count);
            }
        }

        public async Task<SaveProviderFileResult> SaveProviderFile(Stream stream, UserInfo uploadedBy)
        {
            CheckStreamIsProcessable(stream);

            if (await FileIsEmpty(stream))
            {
                return SaveProviderFileResult.EmptyFile();
            }

            if (!await LooksLikeCsv(stream))
            {
                return SaveProviderFileResult.InvalidFile();
            }
            
            var (fileMatchesSchemaResult, missingHeaders) = await FileMatchesSchema<CsvProviderRow>(stream);
            if (fileMatchesSchemaResult == FileMatchesSchemaResult.InvalidHeader)
            {
                    return SaveProviderFileResult.InvalidHeader(missingHeaders);
            }
            
            var providerUploadId = Guid.NewGuid();

            using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher(System.Data.IsolationLevel.ReadCommitted))
            {
                // Check there isn't an existing unprocessed upload for this provider

                var existingUpload = await dispatcher.ExecuteQuery(new GetLatestProviderUploadInProcessing()
                {
                });

                if (existingUpload != null && existingUpload.UploadStatus.IsUnprocessed())
                {
                    return SaveProviderFileResult.ExistingFileInFlight();
                }

                // Abandon any existing un-published upload (there will be one at most)
                if (existingUpload != null)
                {
                    await dispatcher.ExecuteQuery(new SetProviderUploadAbandoned()
                    {
                        ProviderUploadId = existingUpload.ProviderUploadId,
                        AbandonedOn = _clock.UtcNow
                    });
                }

                await dispatcher.ExecuteQuery(new CreateProviderUpload()
                {
                    ProviderUploadId = providerUploadId,
                    CreatedBy = uploadedBy,
                    CreatedOn = _clock.UtcNow,
                });

                await dispatcher.Transaction.CommitAsync();
            }

            await UploadToBlobStorage();

            return SaveProviderFileResult.Success(providerUploadId, UploadStatus.Created);

            async Task UploadToBlobStorage()
            {
                if (!_containerIsKnownToExist)
                {
                    await _blobContainerClient.CreateIfNotExistsAsync();
                    _containerIsKnownToExist = true;
                }                

                var blobName = $"{Constants.ProvidersFolder}/{providerUploadId}.csv";
                await _blobContainerClient.UploadBlobAsync(blobName, stream);
            }
        }

        protected async Task<UploadStatus> GetProviderUploadStatus(Guid providerUploadId)
        {
            using var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher(System.Data.IsolationLevel.ReadCommitted);
            var providerUpload = await dispatcher.ExecuteQuery(new GetProviderUpload() { ProviderUploadId = providerUploadId });

            if (providerUpload == null)
            {
                throw new ArgumentException("Specified providert upload does not exist.", nameof(providerUploadId));
            }

            return providerUpload.UploadStatus;
        }

        // virtual for testing
        protected virtual IObservable<UploadStatus> GetProviderUploadStatusUpdates(Guid providerUploadId) =>
            Observable.Interval(_statusUpdatesPollInterval)
                .SelectMany(_ => Observable.FromAsync(() => GetProviderUploadStatus(providerUploadId)));

        private int GetProviderType(ParsedCsvProviderRow row)
        {
            return (int)ProviderType.FE;
        }

        internal async Task<(UploadStatus uploadStatus, IReadOnlyCollection<ProviderUploadRow> Rows)> ValidateProviderUploadRows(
          ISqlQueryDispatcher sqlQueryDispatcher,
          Guid providerUploadId,
          ProviderDataUploadRowInfoCollection rows)
        {

            var rowsAreValid = true;

            var upsertRecords = new List<UpsertProviderUploadRowsRecord>();

            foreach (var row in rows)
            {
                var rowNumber = row.RowNumber;
                var courseRunId = Guid.NewGuid();

                var parsedRow = ParsedCsvProviderRow.FromCsvProviderRow(row.Data);


                var validator = new ProviderUploadRowValidator(_clock);

                var rowValidationResult = await validator.ValidateAsync(parsedRow);
                var errors = rowValidationResult.Errors.Select(e => e.ErrorCode).ToArray();
                var rowIsValid = rowValidationResult.IsValid;
                rowsAreValid &= rowIsValid;

                upsertRecords.Add(new UpsertProviderUploadRowsRecord()
                {
                    RowNumber = rowNumber,
                    IsValid = rowIsValid,
                    Errors = errors,
                    ProviderId = row.ProviderId,
                    Ukprn = parsedRow.OrgUKPRN,
                    ProviderName = parsedRow.OrgLegalName,
                    TradingName = parsedRow.OrgTradingName,
                    ProviderStatus = 1,
                    ProviderType = GetProviderType(parsedRow),
                    LastUpdated = _clock.UtcNow,
                    LastValidated = _clock.UtcNow,
                });
            }

            var updatedRows = await sqlQueryDispatcher.ExecuteQuery(new UpsertProviderUploadRows()
            {
                ProviderUploadId = providerUploadId,
                ValidatedOn = _clock.UtcNow,
                Records = upsertRecords
            });

            var uploadStatus = await RefreshCourseUploadValidationStatus(providerUploadId, sqlQueryDispatcher);

            return (uploadStatus, updatedRows);
        }

        private async Task<UploadStatus> RefreshProviderUploadValidationStatus(Guid providerUploadId, ISqlQueryDispatcher sqlQueryDispatcher)
        {
            var uploadIsValid = (await sqlQueryDispatcher.ExecuteQuery(new GetProviderUploadInvalidRowCount() { ProviderUploadId = providerUploadId })) == 0;

            await sqlQueryDispatcher.ExecuteQuery(new SetProviderUploadProcessed()
            {
                ProviderUploadId = providerUploadId,
                ProcessingCompletedOn = _clock.UtcNow,
                IsValid = uploadIsValid
            });

            return uploadIsValid ? UploadStatus.ProcessedSuccessfully : UploadStatus.ProcessedWithErrors;
        }

         internal class ProviderUploadRowValidator : AbstractValidator<ParsedCsvProviderRow>
        {
            public ProviderUploadRowValidator(
                IClock clock)
            {
                
            }
        }
    }
}
