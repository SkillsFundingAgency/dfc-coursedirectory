﻿using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries;
using Microsoft.Azure.Documents.Client;
using OneOf.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.QueryHandlers
{
    public class UpdateApprenticeshipHandler : ICosmosDbQueryHandler<UpdateApprenticeship, Success>
    {
        public async Task<Success> Execute(
            DocumentClient client,
            Configuration configuration,
            UpdateApprenticeship request)
        {
            var documentUri = UriFactory.CreateDocumentUri(
                configuration.DatabaseId,
                configuration.ApprenticeshipCollectionName,
                request.Id.ToString());
            
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
                ApprenticeshipLocations = request.ApprenticeshipLocations.Select(l => new ApprenticeshipLocation()
                {
                    Address = l.Address,
                    ApprenticeshipLocationType = l.ApprenticeshipLocationType,
                    CreatedBy = request.CreatedByUser.Email,
                    CreatedDate = request.CreatedDate,
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
                    UpdatedBy = request.CreatedByUser.Email,
                    UpdatedDate = request.CreatedDate,
                    VenueId = l.VenueId
                }).ToList(),
                RecordStatus = 1,
                CreatedDate = request.CreatedDate,
                CreatedBy = request.CreatedByUser.Email,
                UpdatedDate = request.CreatedDate,
                UpdatedBy = request.CreatedByUser.Email,
                BulkUploadErrors = new List<BulkUploadError>(),
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

            await client.ReplaceDocumentAsync(documentUri, apprenticeship);
            return new Success();
        }
    }
}
