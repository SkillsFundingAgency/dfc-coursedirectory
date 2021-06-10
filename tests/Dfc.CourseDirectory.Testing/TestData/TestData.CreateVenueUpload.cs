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
        public async Task<(VenueUpload VenueUpload, VenueUploadRow[] Rows)> CreateVenueUpload(
            Guid providerId,
            UserInfo createdBy,
            UploadStatus uploadStatus,
            Action<VenueUploadRowBuilder> configureRows = null)
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

            var (venueUpload, rows) = await CreateVenueUpload(
                providerId,
                createdBy,
                createdOn,
                processingStartedOn,
                processingCompletedOn,
                publishedOn,
                abandonedOn,
                isValid,
                configureRows);

            Assert.Equal(uploadStatus, venueUpload.UploadStatus);

            return (venueUpload, rows);
        }

        public Task<(VenueUpload VenueUpload, VenueUploadRow[] Rows)> CreateVenueUpload(
            Guid providerId,
            UserInfo createdBy,
            DateTime? createdOn = null,
            DateTime? processingStartedOn = null,
            DateTime? processingCompletedOn = null,
            DateTime? publishedOn = null,
            DateTime? abandonedOn = null,
            bool? isValid = null,
            Action<VenueUploadRowBuilder> configureRows = null)
        {
            if (publishedOn.HasValue && abandonedOn.HasValue)
            {
                throw new ArgumentException($"A {nameof(VenueUpload)} cannot be both {UploadStatus.Abandoned} and {UploadStatus.Published}.");
            }

            var venueUploadId = Guid.NewGuid();
            createdOn ??= _clock.UtcNow;

            return WithSqlQueryDispatcher(async dispatcher =>
            {
                VenueUploadRow[] rows = null;

                await dispatcher.ExecuteQuery(new CreateVenueUpload()
                {
                    VenueUploadId = venueUploadId,
                    ProviderId = providerId,
                    CreatedBy = createdBy,
                    CreatedOn = createdOn.Value
                });

                if (processingStartedOn.HasValue)
                {
                    await dispatcher.ExecuteQuery(new SetVenueUploadProcessing()
                    {
                        VenueUploadId = venueUploadId,
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

                    await dispatcher.ExecuteQuery(new SetVenueUploadProcessed()
                    {
                        VenueUploadId = venueUploadId,
                        ProcessingCompletedOn = processingCompletedOn.Value,
                        IsValid = isValid.Value
                    });

                    var rowBuilder = new VenueUploadRowBuilder();

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
                            rowBuilder.AddRow(record =>
                            {
                                record.VenueName = string.Empty;
                                record.IsValid = false;
                                record.Errors = new[] { ErrorRegistry.All["VENUE_NAME_REQUIRED"].ErrorCode };
                            });
                        }
                    }

                    rows = (await dispatcher.ExecuteQuery(new SetVenueUploadRows()
                    {
                        VenueUploadId = venueUploadId,
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

                    await dispatcher.ExecuteQuery(new PublishVenueUpload()
                    {
                        VenueUploadId = venueUploadId,
                        PublishedBy = createdBy,
                        PublishedOn = publishedOn.Value
                    });
                }
                else if (abandonedOn.HasValue)
                {
                    if (!processingCompletedOn.HasValue)
                    {
                        throw new ArgumentNullException(nameof(processingCompletedOn));
                    }

                    await dispatcher.ExecuteQuery(new SetVenueUploadAbandoned()
                    {
                        VenueUploadId = venueUploadId,
                        AbandonedOn = abandonedOn.Value
                    });
                }

                var venueUpload = await dispatcher.ExecuteQuery(new GetVenueUpload()
                {
                    VenueUploadId = venueUploadId
                });

                return (venueUpload, rows);
            });
        }

        public class VenueUploadRowBuilder
        {
            private readonly List<SetVenueUploadRowsRecord> _records = new List<SetVenueUploadRowsRecord>();

            public VenueUploadRowBuilder AddRow(Action<SetVenueUploadRowsRecord> configureRecord)
            {
                var record = CreateValidRecord();
                configureRecord(record);
                _records.Add(record);
                return this;
            }

            public VenueUploadRowBuilder AddRow(
                Guid venueId,
                string providerVenueRef,
                string venueName,
                string addressLine1,
                string addressLine2,
                string town,
                string county,
                string postcode,
                string email,
                string telephone,
                string website,
                IEnumerable<string> errors = null,
                bool isSupplementary = false,
                bool isDeletable = true)
            {
                var record = CreateRecord(
                    venueId,
                    providerVenueRef,
                    venueName,
                    addressLine1,
                    addressLine2,
                    town,
                    county,
                    postcode,
                    email,
                    telephone,
                    website,
                    errors,
                    isSupplementary,
                    isDeletable);

                _records.Add(record);

                return this;
            }

            public VenueUploadRowBuilder AddValidRow(bool isSupplementary = false, Guid? venueId = null)
            {
                var record = CreateValidRecord(isSupplementary, venueId);
                _records.Add(record);
                return this;
            }

            public VenueUploadRowBuilder AddValidRows(int count)
            {
                for (int i = 0; i < count; i++)
                {
                    AddValidRow();
                }

                return this;
            }

            internal IReadOnlyCollection<SetVenueUploadRowsRecord> GetUpsertQueryRows() => _records;

            private SetVenueUploadRowsRecord CreateRecord(
                Guid venueId,
                string providerVenueRef,
                string venueName,
                string addressLine1,
                string addressLine2,
                string town,
                string county,
                string postcode,
                string email,
                string telephone,
                string website,
                IEnumerable<string> errors = null,
                bool isSupplementary = false,
                bool isDeletable = true)
            {
                var errorsArray = errors?.ToArray() ?? Array.Empty<string>();
                var isValid = !errorsArray.Any();

                return new SetVenueUploadRowsRecord()
                {
                    RowNumber = _records.Count + 2,
                    IsValid = isValid,
                    Errors = errorsArray,
                    IsSupplementary = isSupplementary,
                    VenueId = venueId,
                    IsDeletable = isDeletable,
                    ProviderVenueRef = providerVenueRef,
                    VenueName = venueName,
                    AddressLine1 = addressLine1,
                    AddressLine2 = addressLine2,
                    Town = town,
                    County = county,
                    Postcode = postcode,
                    Email = email,
                    Telephone = telephone,
                    Website = website
                };
            }

            private SetVenueUploadRowsRecord CreateValidRecord(
                bool isSupplementary = false,
                Guid? venueId = null,
                bool isDeletable = true)
            {
                string venueName;
                do
                {
                    venueName = Faker.Company.Name();
                }
                while (_records.Any(r => r.VenueName == venueName));

                return CreateRecord(
                    venueId: venueId ?? Guid.NewGuid(),
                    providerVenueRef: Guid.NewGuid().ToString(),
                    venueName,
                    addressLine1: Faker.Address.StreetAddress(),
                    addressLine2: Faker.Address.SecondaryAddress(),
                    town: Faker.Address.City(),
                    county: Faker.Address.UkCounty(),
                    postcode: "AB1 2DE",  // Faker's method sometimes produces invalid postcodes :-/
                    email: Faker.Internet.Email(),
                    telephone: "01234 567890",  // There's no Faker method for a UK phone number
                    website: Faker.Internet.Url(),
                    isSupplementary: isSupplementary,
                    isDeletable: isDeletable);
            }
        }
    }
}
