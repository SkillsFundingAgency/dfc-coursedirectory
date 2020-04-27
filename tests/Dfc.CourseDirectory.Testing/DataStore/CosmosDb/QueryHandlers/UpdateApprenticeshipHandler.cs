using Dfc.CourseDirectory.Core;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using OneOf.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dfc.CourseDirectory.Testing.DataStore.CosmosDb.QueryHandlers
{
    public class UpdateApprenticeshipHandler : ICosmosDbQueryHandler<UpdateApprenticeship, Success>
    {
        public Success Execute(InMemoryDocumentStore inMemoryDocumentStore, UpdateApprenticeship request)
        {
            var apprenticeship = inMemoryDocumentStore.Apprenticeships.All.SingleOrDefault(p => p.Id == request.Id);

            if (apprenticeship != null)
            {
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
                    CreatedBy = request.UpdatedBy.Email,
                    CreatedDate = request.UpdatedDate,
                    DeliveryModes = EnumHelper.SplitFlags(l.ApprenticeshipLocationType).Cast<int>().ToList(),
                    Id = Guid.NewGuid(),
                    LocationType = l.LocationType,
                    Name = l.Name,
                    National = l.National,
                    Phone = l.Phone,
                    ProviderUKPRN = request.ProviderUkprn,
                    Radius = l.Radius,
                    RecordStatus = 1,
                    Regions = l.Regions,
                    UpdatedBy = request.UpdatedBy.Email,
                    UpdatedDate = request.UpdatedDate,
                    VenueId = l.VenueId
                }).ToList();
                apprenticeship.RecordStatus = 1;
                apprenticeship.UpdatedDate = request.UpdatedDate;
                apprenticeship.UpdatedBy = request.UpdatedBy.Email;
                apprenticeship.BulkUploadErrors = new List<BulkUploadError>();
                apprenticeship.ValidationErrors = Array.Empty<string>();
                apprenticeship.LocationValidationErrors = Array.Empty<string>();

                request.StandardOrFramework.Switch(
                    standard =>
                    {
                        apprenticeship.StandardId = standard.CosmosId;
                        apprenticeship.StandardCode = standard.StandardCode;
                        apprenticeship.Version = standard.Version;
                        apprenticeship.NotionalNVQLevelv2 = standard.NotionalNVQLevelv2;
                    },
                    framework =>
                    {
                        apprenticeship.FrameworkId = framework.CosmosId;
                        apprenticeship.FrameworkCode = framework.FrameworkCode;
                        apprenticeship.ProgType = framework.ProgType;
                        apprenticeship.PathwayCode = framework.PathwayCode;
                    });

                inMemoryDocumentStore.Apprenticeships.Save(apprenticeship);
            }
            return new Success();
        }

    }
}
