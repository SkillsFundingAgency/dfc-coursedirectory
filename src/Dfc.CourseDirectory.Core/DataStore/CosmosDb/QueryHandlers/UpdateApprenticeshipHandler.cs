using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.QueryHandlers
{
    public class UpdateApprenticeshipHandler : ICosmosDbQueryHandler<UpdateApprenticeship, OneOf<NotFound, Success>>
    {
        public async Task<OneOf<NotFound, Success>> Execute(DocumentClient client,
            Configuration configuration, UpdateApprenticeship request)
        {
            var documentUri = UriFactory.CreateDocumentUri(
                configuration.DatabaseId,
                configuration.ApprenticeshipCollectionName,
                request.Id.ToString());

            var partitionKey = new PartitionKey(request.ProviderUkprn);

            Apprenticeship apprenticeship;

            try
            {
                var query = await client.ReadDocumentAsync<Apprenticeship>(
                    documentUri,
                    new RequestOptions() { PartitionKey = partitionKey });

                apprenticeship = query.Document;
            }
            catch (DocumentClientException dex) when (dex.StatusCode == HttpStatusCode.NotFound)
            {
                return new NotFound();
            }

            apprenticeship.ApprenticeshipTitle = request.ApprenticeshipTitle;
            apprenticeship.ApprenticeshipType = request.ApprenticeshipType;
            apprenticeship.MarketingInformation = request.MarketingInformation;
            apprenticeship.Url = request.Url;
            apprenticeship.ContactTelephone = request.ContactTelephone;
            apprenticeship.ContactEmail = request.ContactEmail;
            apprenticeship.ContactWebsite = request.ContactWebsite;
            apprenticeship.ApprenticeshipLocations = request.ApprenticeshipLocations.Select(l => new ApprenticeshipLocation()
            {
                Address = l.Address,
                ApprenticeshipLocationType = l.ApprenticeshipLocationType,
                CreatedBy = request.UpdatedBy.UserId,
                CreatedDate = request.UpdatedDate,
                DeliveryModes = l.DeliveryModes.ToList(),
                Id = l.Id ?? Guid.NewGuid(),
                LocationType = l.LocationType,
                Name = l.Name,
                National = l.National,
                Phone = l.Phone,
                ProviderUKPRN = apprenticeship.ProviderUKPRN,
                Radius = l.Radius,
                RecordStatus = 1,
                Regions = l.Regions,
                UpdatedBy = request.UpdatedBy.UserId,
                UpdatedDate = request.UpdatedDate,
                VenueId = l.VenueId
            }).ToList();
            apprenticeship.RecordStatus = request.Status ?? apprenticeship.RecordStatus;
            apprenticeship.UpdatedDate = request.UpdatedDate;
            apprenticeship.UpdatedBy = request.UpdatedBy.UserId;
            apprenticeship.BulkUploadErrors = request.BulkUploadErrors?.ToList() ?? apprenticeship.BulkUploadErrors;
            apprenticeship.StandardId = request.Standard.CosmosId;
            apprenticeship.StandardCode = request.Standard.StandardCode;
            apprenticeship.Version = request.Standard.Version;
            apprenticeship.NotionalNVQLevelv2 = request.Standard.NotionalNVQLevelv2;

            await client.ReplaceDocumentAsync(
                documentUri,
                apprenticeship,
                new RequestOptions() { PartitionKey = partitionKey });

            return new Success();
        }
    }
}
