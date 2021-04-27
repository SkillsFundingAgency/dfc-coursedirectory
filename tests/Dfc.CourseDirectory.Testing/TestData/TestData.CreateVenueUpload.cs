﻿using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Xunit;

namespace Dfc.CourseDirectory.Testing
{
    public partial class TestData
    {
        public async Task<VenueUpload> CreateVenueUpload(
            Guid providerId,
            UserInfo createdBy,
            UploadStatus uploadStatus,
            bool? isValid = null)
        {
            var createdOn = _clock.UtcNow;

            DateTime? processingStartedOn = uploadStatus >= UploadStatus.InProgress ? createdOn.AddSeconds(3) : (DateTime?)null;
            DateTime? processingCompletedOn = uploadStatus >= UploadStatus.Processed ? processingStartedOn.Value.AddSeconds(30) : (DateTime?)null; 
            DateTime? publishedOn = uploadStatus == UploadStatus.Published ? processingCompletedOn.Value.AddHours(2) : (DateTime?)null;
            DateTime? abandonedOn = uploadStatus == UploadStatus.Abandoned ? processingCompletedOn.Value.AddHours(2) : (DateTime?)null;

            if (processingCompletedOn.HasValue && !isValid.HasValue)
            {
                isValid = true;
            }

            var venueUpload = await CreateVenueUpload(
                providerId,
                createdBy,
                createdOn,
                processingStartedOn,
                processingCompletedOn,
                publishedOn,
                abandonedOn,
                isValid);

            Assert.Equal(uploadStatus, venueUpload.UploadStatus);

            return venueUpload;
        }

        public Task<VenueUpload> CreateVenueUpload(
            Guid providerId,
            UserInfo createdBy,
            DateTime? createdOn = null,
            DateTime? processingStartedOn = null,
            DateTime? processingCompletedOn = null,
            DateTime? publishedOn = null,
            DateTime? abandonedOn = null,
            bool? isValid = null)
        {
            if (publishedOn.HasValue && abandonedOn.HasValue)
            {
                throw new ArgumentException($"A {nameof(VenueUpload)} cannot be both {UploadStatus.Abandoned} and {UploadStatus.Published}.");
            }

            var venueUploadId = Guid.NewGuid();
            createdOn ??= _clock.UtcNow;

            return WithSqlQueryDispatcher(async dispatcher =>
            {
                await dispatcher.ExecuteQuery(new CreateVenueUpload()
                {
                    VenueUploadId = venueUploadId,
                    ProviderId = providerId,
                    CreatedBy = createdBy,
                    CreatedOn = createdOn.Value
                });

                if (processingStartedOn.HasValue)
                {
                    await dispatcher.ExecuteQuery(new SetVenueUploadInProgress()
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
                }

                if (publishedOn.HasValue)
                {
                    if (!processingCompletedOn.HasValue)
                    {
                        throw new ArgumentNullException(nameof(processingCompletedOn));
                    }

                    await dispatcher.ExecuteQuery(new SetVenueUploadPublished()
                    {
                        VenueUploadId = venueUploadId,
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

                return await dispatcher.ExecuteQuery(new GetVenueUpload()
                {
                    VenueUploadId = venueUploadId
                });
            });
        }
    }
}
