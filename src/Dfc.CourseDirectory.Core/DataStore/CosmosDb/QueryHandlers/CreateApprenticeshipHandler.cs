using System;
using System.Linq;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Microsoft.Azure.Documents.Client;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.QueryHandlers
{
    public class CreateApprenticeshipHandler : ICosmosDbQueryHandler<CreateApprenticeship, Success>
    {
        public async Task<Success> Execute(DocumentClient client, Configuration configuration, CreateApprenticeship request)
        {
            var documentUri = UriFactory.CreateDocumentCollectionUri(
                configuration.DatabaseId,
                configuration.ApprenticeshipCollectionName);

            var apprenticeship = new Apprenticeship()
            {
                Id = request.Id,
                ProviderId = request.ProviderId,
                ProviderUKPRN = request.ProviderUkprn,
                ApprenticeshipTitle = request.ApprenticeshipTitle,
                ApprenticeshipType = request.ApprenticeshipType,
                MarketingInformation = request.MarketingInformation,
                Url = request.Url,
                ContactTelephone = request.ContactTelephone,
                ContactEmail = request.ContactEmail,
                ContactWebsite = request.ContactWebsite,
                ApprenticeshipLocations = request.ApprenticeshipLocations.Select(l => new ApprenticeshipLocation()
                {
                    Address = l.Address,
                    ApprenticeshipLocationType = l.ApprenticeshipLocationType,
                    CreatedBy = request.CreatedByUser.UserId,
                    CreatedDate = request.CreatedDate,
                    DeliveryModes = l.DeliveryModes.ToList(),
                    Id = l.Id ?? Guid.NewGuid(),
                    LocationType = l.LocationType,
                    Name = l.Name,
                    National = l.National,
                    Phone = l.Phone,
                    ProviderUKPRN = request.ProviderUkprn,
                    Radius = l.Radius,
                    RecordStatus = 1,
                    Regions = l.Regions,
                    UpdatedBy = request.CreatedByUser.UserId,
                    UpdatedDate = request.CreatedDate,
                    VenueId = l.VenueId
                }).ToList(),
                RecordStatus = request.Status,
                CreatedDate = request.CreatedDate,
                CreatedBy = request.CreatedByUser.UserId,
                UpdatedDate = request.CreatedDate,
                UpdatedBy = request.CreatedByUser.UserId,
                BulkUploadErrors = request.BulkUploadErrors?.ToList(),
                StandardId = request.Standard.CosmosId,
                StandardCode = request.Standard.StandardCode,
                Version = request.Standard.Version,
                NotionalNVQLevelv2 = request.Standard.NotionalNVQLevelv2
            };

            await client.CreateDocumentAsync(documentUri, apprenticeship);

            return new Success();
        }
    }
}
