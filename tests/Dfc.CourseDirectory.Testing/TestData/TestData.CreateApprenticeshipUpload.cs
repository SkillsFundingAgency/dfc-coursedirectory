using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.Core.Validation;
using Xunit;

namespace Dfc.CourseDirectory.Testing
{
    public partial class TestData
    {
        public async Task<(ApprenticeshipUpload apprenticeshipUpload, ApprenticeshipUploadRow[] Rows)> CreateApprenticeshipUpload(
            Guid providerId,
            UserInfo createdBy,
            UploadStatus uploadStatus,
            Action<ApprenticeshipUploadRowBuilder> configureRows = null)
        {
            var createdOn = _clock.UtcNow;

            DateTime? processingStartedOn = uploadStatus >= UploadStatus.Processing ? createdOn.AddSeconds(3) : (DateTime?)null;
            DateTime? processingCompletedOn = uploadStatus >= UploadStatus.ProcessedWithErrors ? processingStartedOn.Value.AddSeconds(30) : (DateTime?)null;
            DateTime? publishedOn = uploadStatus == UploadStatus.Published ? processingCompletedOn.Value.AddHours(2) : (DateTime?)null;
            DateTime? abandonedOn = uploadStatus == UploadStatus.Abandoned ? processingCompletedOn.Value.AddHours(2) : (DateTime?)null;

            var isValid = uploadStatus switch
            {
                UploadStatus.ProcessedWithErrors => false,
                UploadStatus.Created | UploadStatus.Processing => (bool?)null,
                _ => true
            };

            var (courseUpload, rows) = await CreateApprenticeshipUpload(
                providerId,
                createdBy,
                createdOn,
                processingStartedOn,
                processingCompletedOn,
                publishedOn,
                abandonedOn,
                isValid,
                configureRows);

            Assert.Equal(uploadStatus, courseUpload.UploadStatus);

            return (courseUpload, rows);
        }

        public Task<(ApprenticeshipUpload apprenticeshipUpload, ApprenticeshipUploadRow[] Rows)> CreateApprenticeshipUpload(
            Guid providerId,
            UserInfo createdBy,
            DateTime? createdOn = null,
            DateTime? processingStartedOn = null,
            DateTime? processingCompletedOn = null,
            DateTime? publishedOn = null,
            DateTime? abandonedOn = null,
            bool? isValid = null,
            Action<ApprenticeshipUploadRowBuilder> configureRows = null)
        {
            var apprenticeshipUploadId = Guid.NewGuid();
            createdOn ??= _clock.UtcNow;

            return WithSqlQueryDispatcher(async dispatcher =>
            {
                ApprenticeshipUploadRow[] rows = null;

                await dispatcher.ExecuteQuery(new CreateApprenticeshipUpload()
                {
                    ApprenticeshipUploadId = apprenticeshipUploadId,
                    ProviderId = providerId,
                    CreatedBy = createdBy,
                    CreatedOn = createdOn.Value
                });

                if (processingStartedOn.HasValue)
                {
                    await dispatcher.ExecuteQuery(new SetApprenticeshipUploadProcessing()
                    {
                        ApprenticeshipUploadId = apprenticeshipUploadId,
                        ProcessingStartedOn = processingStartedOn.Value
                    });
                }

                if (processingCompletedOn.HasValue)
                {
                    if (!processingStartedOn.HasValue)
                    {
                        throw new ArgumentNullException(nameof(processingStartedOn));
                    }

                    if (!isValid.HasValue)
                    {
                        throw new ArgumentNullException(nameof(isValid));
                    }

                    await dispatcher.ExecuteQuery(new SetApprenticeshipUploadProcessed()
                    {
                        ApprenticeshipUploadId = apprenticeshipUploadId,
                        ProcessingCompletedOn = processingCompletedOn.Value,
                        IsValid = isValid.Value
                    });

                    var rowBuilder = new ApprenticeshipUploadRowBuilder();

                    if (configureRows != null)
                    {
                        configureRows(rowBuilder);
                    }
                    else
                    {
                        if (isValid.Value)
                        {
                            rowBuilder.AddValidRows(3);
                        }
                        else
                        {
                            rowBuilder.AddRow(learnAimRef: string.Empty, record =>
                            {
                                record.IsValid = false;
                                record.Errors = new[] { ErrorRegistry.All["COURSE_LARS_QAN_REQUIRED"].ErrorCode };
                            });
                        }
                    }

                    rows = (await dispatcher.ExecuteQuery(new UpsertApprenticeshipUploadRows()
                    {
                        ApprenticeshipUploadId = apprenticeshipUploadId,
                        Records = rowBuilder.GetUpsertQueryRows(),
                        UpdatedOn = processingCompletedOn.Value,
                        ValidatedOn = processingCompletedOn.Value
                    })).ToArray();
                }

                if (publishedOn.HasValue)
                {
                    if (!processingCompletedOn.HasValue)
                    {
                        throw new ArgumentNullException(nameof(processingCompletedOn));
                    }

                    await dispatcher.ExecuteQuery(new SetApprenticeshipUploadProcessed()
                    {
                        ApprenticeshipUploadId = apprenticeshipUploadId,
                        ProcessingCompletedOn = processingCompletedOn.Value,
                        IsValid = isValid.Value
                    });
                }
                else if (abandonedOn.HasValue)
                {
                    if (!processingCompletedOn.HasValue)
                    {
                        throw new ArgumentNullException(nameof(processingCompletedOn));
                    }

                    await dispatcher.ExecuteQuery(new SetApprenticeshipUploadAbandoned()
                    {
                        ApprenticeshipUploadId = apprenticeshipUploadId,
                        AbandonedOn = abandonedOn.Value
                    });
                }

                var apprenticeshipUpload = await dispatcher.ExecuteQuery(new GetApprenticeshipUpload()
                {
                    ApprenticeshipUploadId = apprenticeshipUploadId
                });

                return (apprenticeshipUpload, rows);
            });
        }

        public class ApprenticeshipUploadRowBuilder
        {
            private readonly List<UpsertApprenticeshipUploadRowsRecord> _records = new List<UpsertApprenticeshipUploadRowsRecord>();

            public ApprenticeshipUploadRowBuilder AddRow(string learnAimRef, Action<UpsertApprenticeshipUploadRowsRecord> configureRecord)
            {
                var record = CreateValidRecord();
                configureRecord(record);
                _records.Add(record);
                return this;
            }

            public ApprenticeshipUploadRowBuilder AddRow(
                Guid apprenticeshipId,
                int standardCode,
                int standardVersion,
                string apprenticeshipInformation,
                string apprenticeshipWebpage,
                string contactEmail,
                string contactPhone,
                string contactUrl,
                string deliveryMode,
                string venue,
                string yourVenueReference,
                string radius,
                string nationalDelivery,
                string subRegion,
                IEnumerable<string> errors = null)
            {
                var record = CreateRecord(
                apprenticeshipId,
                standardCode,
                standardVersion,
                apprenticeshipInformation,
                apprenticeshipWebpage,
                contactEmail,
                contactPhone,
                contactUrl,
                deliveryMode,
                venue,
                yourVenueReference,
                radius,
                nationalDelivery,
                subRegion,
                errors);
                _records.Add(record);
                return this;
            }

            public ApprenticeshipUploadRowBuilder AddValidRow()
            {
                var record = CreateValidRecord();
                _records.Add(record);
                return this;
            }

            public ApprenticeshipUploadRowBuilder AddValidRows(int count)
            {
                for (int i = 0; i < count; i++)
                {
                    AddValidRow();
                }

                return this;
            }

            internal IReadOnlyCollection<UpsertApprenticeshipUploadRowsRecord> GetUpsertQueryRows() => _records;

            private UpsertApprenticeshipUploadRowsRecord CreateRecord(
                Guid apprenticeshipId,
                int standardCode,
                int standardVersion,
                string apprenticeshipInformation,
                string apprenticeshipWebpage,
                string contactEmail,
                string contactPhone,
                string contactUrl,
                string deliveryMode,
                string venue,
                string yourVenueReference,
                string radius,
                string nationalDelivery,
                string subRegion,
                IEnumerable<string> errors = null)
            {
                var errorsArray = errors?.ToArray() ?? Array.Empty<string>();
                var isValid = !errorsArray.Any();

                return new UpsertApprenticeshipUploadRowsRecord()
                {
                    RowNumber = _records.Count + 2,
                    IsValid = isValid,
                    ApprenticeshipId = apprenticeshipId,
                    StandardCode = standardCode,
                    StandardVersion = standardVersion,
                    ApprenticeshipInformation = apprenticeshipInformation,
                    ApprenticeshipWebpage = apprenticeshipWebpage,
                    ContactEmail = contactEmail,
                    ContactPhone = contactPhone,
                    ContactUrl = contactUrl,
                    DeliveryMode = deliveryMode,
                    Venue = venue,
                    YourVenueReference = yourVenueReference,
                    Radius = radius,
                    NationalDelivery = nationalDelivery,
                    SubRegion = subRegion,
                    Errors = errors
                };
            }

            private UpsertApprenticeshipUploadRowsRecord CreateValidRecord()
            {
                return CreateRecord(
                    apprenticeshipId: Guid.NewGuid(),
                    standardCode: 1,
                    standardVersion: 1,
                    apprenticeshipInformation: "Some info",
                    apprenticeshipWebpage: "https://someapprenticeshipsite.com",
                    contactEmail: "",
                    contactPhone: "",
                    contactUrl: "",
                    deliveryMode: "1",
                    venue: "Some venue",
                    yourVenueReference: "Some Reference",
                    radius: "1",
                    nationalDelivery: "1",
                    subRegion: "");
            }
        }
    }
}
