using System;
using System.Linq;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Testing.DataStore.CosmosDb.QueryHandlers
{
    public class UpdateApprenticeshipHandler : ICosmosDbQueryHandler<UpdateApprenticeship, OneOf<NotFound, Success>>
    {
        public OneOf<NotFound, Success> Execute(
            InMemoryDocumentStore inMemoryDocumentStore,
            UpdateApprenticeship request)
        {
            var apprenticeship = inMemoryDocumentStore.Apprenticeships.All.SingleOrDefault(p => p.Id == request.Id);

            if (apprenticeship == null)
            {
                return new NotFound();
            }

            apprenticeship.ProviderUKPRN = request.ProviderUkprn;
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
                ProviderUKPRN = request.ProviderUkprn,
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

            inMemoryDocumentStore.Apprenticeships.Save(apprenticeship);

            return new Success();
        }
    }
}
