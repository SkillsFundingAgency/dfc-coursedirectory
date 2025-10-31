using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Security.Policy;
using System.ServiceModel.Channels;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp.Css.Values;
using Dfc.CourseDirectory.Core.DataManagement.Schemas;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Helpers;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Search.Models;
using Dfc.CourseDirectory.Core.Services;
using Dfc.CourseDirectory.Core.Validation.CourseValidation;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataManagement
{
    public partial class FileUploadProcessor
    {
        //TODO Could be moved to cofig or DB
        private string[] FundNames = new string[]
        {
            "14-16 in FE",
            "16-19 Funding",
            "16-19 Learner Responsive Funding",
            "24+ Advanced Learning Loans Bursary",
            "24+ Advanced Learning Loans Bursary (College)",
            "24+ Advanced Learning Loans Facility",
            "Academy Funding",
            "Adult Education Budget (Procured)",
            "Adult Skills (College)",
            "AEB Devolution",
            "Community Learning Funding",
            "Multiply",
            "Non-Mainstream Funding EFA",
            "School Sixth Form Funding",
            "Skills Boot Camp",
        };
        public async Task ProcessProviderFile(Guid providerUploadId, Stream stream)
        {
            bool inactiveProviders = false;
            using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher(System.Data.IsolationLevel.ReadCommitted))
            {
                var uploadedCourse = await dispatcher.ExecuteQuery(new GetProviderUpload() { ProviderUploadId = providerUploadId });
                inactiveProviders = uploadedCourse != null ? uploadedCourse.InactiveProviders: false;

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
            if (inactiveProviders)
            {
                List<CsvInactiveProviderRow> rows;
                using (var streamReader = new StreamReader(stream))
                using (var csvReader = CreateCsvReader(streamReader))
                {
                    rows = await csvReader.GetRecordsAsync<CsvInactiveProviderRow>().ToListAsync();
                }
                var grouped = CsvInactiveProviderRow.GroupRows(rows);
                var groupProvidersIds = grouped.Select(g => (ProviderId: Guid.NewGuid(), Rows: g)).ToArray();

                var rowInfos = new List<InactiveProviderDataUploadRowInfo>(rows.Count);

                foreach (var row in rows)
                {
                    var providerId = groupProvidersIds.Single(g => g.Rows.Contains(row)).ProviderId;


                    rowInfos.Add(new InactiveProviderDataUploadRowInfo(row, rowNumber: rowInfos.Count + 2, providerId));
                }

                var rowsCollection = new InactiveProviderDataUploadRowInfoCollection(rowInfos);

                using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher(System.Data.IsolationLevel.ReadCommitted))
                {

                    await ValidateInactiveProviderUploadRows(dispatcher, providerUploadId, rowsCollection);

                    await dispatcher.Commit();
                }
                await InactiveProviderUpdate(providerUploadId);
            }
            else
            {
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

                    await ValidateProviderUploadRows(dispatcher, providerUploadId, rowsCollection);

                    await dispatcher.Commit();
                }
                await OnboardProviderUpload(providerUploadId);

            }

            await DeleteBlob();

            Task DeleteBlob()
            {
                var blobName = $"{Constants.ProvidersFolder}/{providerUploadId}.csv";
                return _blobContainerClient.DeleteBlobIfExistsAsync(blobName);
            }
        }

        public async Task<OnboardResult> InactiveProviderUpdate(Guid providerUploadId)
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

                var onboardProviderResult = await dispatcher.ExecuteQuery(new InactiveProviderUpload()
                {
                    ProviderUploadId = providerUpload.ProviderUploadId,
                    UpdatedOn = onboardedOn
                });

                await dispatcher.Commit();

                var onboardedProviderIds = onboardProviderResult.AsT1.OnboardedProviderIds;

                return OnboardResult.Success(onboardedProviderIds.Count);
            }
        }

        public async Task<OnboardResult> OnboardProviderUpload(Guid providerUploadId)
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

                var onboardProviderResult = await dispatcher.ExecuteQuery(new OnboardProviderUpload()
                {
                    ProviderUploadId = providerUpload.ProviderUploadId,
                    OnboardedOn = onboardedOn
                });

                await dispatcher.Commit();

                var onboardedProviderIds = onboardProviderResult.AsT1.OnboardedProviderIds;

                return OnboardResult.Success(onboardedProviderIds.Count);
            }
        }

        public async Task<SaveProviderFileResult> SaveProviderFile(Stream stream, bool inactiveProviders, UserInfo uploadedBy)
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

            if (inactiveProviders) 
            {
                var (fileMatchesSchemaResult, missingHeaders) = await FileMatchesSchema<CsvInactiveProviderRow>(stream);
                if (fileMatchesSchemaResult == FileMatchesSchemaResult.InvalidHeader)
                {
                    return SaveProviderFileResult.InvalidHeader(missingHeaders);
                }
            }
            else
            {
                var (fileMatchesSchemaResult, missingHeaders) = await FileMatchesSchema<CsvProviderRow>(stream);
                if (fileMatchesSchemaResult == FileMatchesSchemaResult.InvalidHeader)
                {
                    return SaveProviderFileResult.InvalidHeader(missingHeaders);
                }
            }
            
            
            var providerUploadId = Guid.NewGuid();

            using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher(System.Data.IsolationLevel.ReadCommitted))
            {

                await dispatcher.ExecuteQuery(new CreateProviderUpload()
                {
                    ProviderUploadId = providerUploadId,
                    CreatedBy = uploadedBy,
                    CreatedOn = _clock.UtcNow,
                    InactiveProviders = inactiveProviders,
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

        private int GetProviderType(ParsedCsvProviderRow row, string[] fundNames)
        {
            var providerType = 0;
            if (fundNames.Any(fName => string.Equals(fName, "Skills Boot Camp", StringComparison.CurrentCultureIgnoreCase)))
            {
                    providerType = providerType + (int)ProviderType.NonLARS;

            }
            if (fundNames.Any(fName => !string.Equals(fName, "Skills Boot Camp", StringComparison.CurrentCultureIgnoreCase)))
            {
                providerType = providerType + (int)ProviderType.FE;

            }

            return providerType;
        }

        internal async Task<(UploadStatus uploadStatus, IReadOnlyCollection<ProviderUploadRow> Rows)> ValidateInactiveProviderUploadRows(
          ISqlQueryDispatcher sqlQueryDispatcher,
          Guid providerUploadId,
          InactiveProviderDataUploadRowInfoCollection rows)
        {

            var rowsAreValid = true;

            var upsertRecords = new List<UpsertInactiveProviderUploadRowsRecord>();
            var filterByPreviousMonth = DateTime.Now.Month - 1;

            foreach (var row in rows)
            {
                var rowNumber = row.RowNumber;

                var parsedRow = ParsedCsvInactiveProviderRow.FromCsvProviderRow(row.Data);
                if (upsertRecords.Any(x => x.Ukprn == parsedRow.OrgUKPRN))
                {
                    continue;
                }
               
                var validator = new InactiveProviderUploadRowValidator(_clock);
                var rowValidationResult = await validator.ValidateAsync(parsedRow);
                var errors = rowValidationResult.Errors.Select(e => e.ErrorCode).ToArray();
                var rowIsValid = rowValidationResult.IsValid;
                rowsAreValid &= rowIsValid;

                upsertRecords.Add(new UpsertInactiveProviderUploadRowsRecord()
                {
                    RowNumber = rowNumber,
                    IsValid = rowIsValid,
                    Errors = errors,
                    ProviderId = row.ProviderId,
                    Ukprn = parsedRow.OrgUKPRN,
                    ProviderName = parsedRow.OrgLegalName,
                    OrgStatus = parsedRow.OrgStatus,
                    OrgStatusDate = parsedRow.ResolvedOrgStatusDate,
                    LastUpdated = _clock.UtcNow,
                    LastValidated = _clock.UtcNow,
                });
            }

            var updatedRows = await sqlQueryDispatcher.ExecuteQuery(new UpsertInactiveProviderUploadRows()
            {
                ProviderUploadId = providerUploadId,
                ValidatedOn = _clock.UtcNow,
                Records = upsertRecords
            });

            var uploadStatus = await RefreshProviderUploadValidationStatus(providerUploadId, sqlQueryDispatcher);

            return (uploadStatus, updatedRows);
        }

        internal async Task<(UploadStatus uploadStatus, IReadOnlyCollection<ProviderUploadRow> Rows)> ValidateProviderUploadRows(
          ISqlQueryDispatcher sqlQueryDispatcher,
          Guid providerUploadId,
          ProviderDataUploadRowInfoCollection rows)
        {

            var rowsAreValid = true;

            var upsertRecords = new List<UpsertProviderUploadRowsRecord>();
            var filterByPreviousMonth = DateTime.Now.Month -1;

            foreach (var row in rows)
            {
                var rowNumber = row.RowNumber;

                var parsedRow = ParsedCsvProviderRow.FromCsvProviderRow(row.Data);
                if (upsertRecords.Any(x => x.Ukprn == parsedRow.OrgUKPRN)) {
                    continue;
                }
                var fundNames = rows.Where(x => x.Data.OrgUKPRN ==  parsedRow.OrgUKPRN).Select(x => x.Data.FundName).ToArray();
                var firstDayOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month - 1, 1);
                var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
                var isPreviousMOnthData = parsedRow.ResolvedFundStartDate.HasValue &&
                parsedRow.ResolvedFundStartDate.Value >= firstDayOfMonth &&
                parsedRow.ResolvedFundStartDate <= lastDayOfMonth;
                var allowedFundNames = FundNames.Any(x => string.Equals(parsedRow.FundName, x, StringComparison.InvariantCultureIgnoreCase));
                if (!isPreviousMOnthData || !allowedFundNames)
                {
                    continue;
                } 

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
                    ProviderType = GetProviderType(parsedRow, fundNames),
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

            var uploadStatus = await RefreshProviderUploadValidationStatus(providerUploadId, sqlQueryDispatcher);

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

        internal class InactiveProviderUploadRowValidator : AbstractValidator<ParsedCsvInactiveProviderRow>
        {
            public InactiveProviderUploadRowValidator(
                IClock clock)
            {

            }
        }
    }
}
