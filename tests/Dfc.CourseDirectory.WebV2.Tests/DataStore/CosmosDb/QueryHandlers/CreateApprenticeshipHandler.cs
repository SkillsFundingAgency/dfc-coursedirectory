using System;
using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.WebV2.Models;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Tests.DataStore.CosmosDb.QueryHandlers
{
    public class CreateApprenticeshipHandler : ICosmosDbQueryHandler<CreateApprenticeship, Success>
    {
        public Success Execute(InMemoryDocumentStore inMemoryDocumentStore, CreateApprenticeship request)
        {
            var apprenticeship = new Apprenticeship()
            {
                Id = request.Id,
                ProviderUKPRN = request.ProviderUkprn,
                ApprenticeshipTitle = request.ApprenticeshipTitle,
                ApprenticeshipType = request.ApprenticeshipType,
                MarketingInformation = request.MarketingInformation,
                Url = request.Url,
                ContactTelephone = request.ContactTelephone,
                ContactEmail = request.ContactEmail,
                ContactWebsite = request.ContactWebsite,
                ApprenticeshipLocations = request.ApprenticeshipLocations.Select(l => l.ApprenticeshipLocationType switch
                {
                    ApprenticeshipLocationType.EmployerBased => new ApprenticeshipLocation()
                    {
                        Id = Guid.NewGuid(),
                        National = true,  // FIXME when we support regions
                        DeliveryModes = new List<int>() { 1 },
                        ProviderUKPRN = request.ProviderUkprn,
                        Regions = Array.Empty<string>(),
                        ApprenticeshipLocationType = l.ApprenticeshipLocationType
                    },
                    _ => throw new NotImplementedException()
                }).ToList(),
                RecordStatus = 1,
                CreatedDate = request.CreatedDate,
                CreatedBy = request.CreatedByUser.Email,
                UpdatedDate = request.CreatedDate,
                UpdatedBy = request.CreatedByUser.Email,
                BulkUploadErrors = new System.Collections.Generic.List<BulkUploadError>(),
                ValidationErrors = Array.Empty<string>(),
                LocationValidationErrors = Array.Empty<string>()
            };

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

            return new Success();
        }
    }
}
