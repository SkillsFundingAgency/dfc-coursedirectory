﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.WebV2.Behaviors.Errors;
using MediatR;

namespace Dfc.CourseDirectory.WebV2.Features.NewApprenticeshipProvider.ProviderDetailInfoPanel
{
    public class Query : IRequest<ViewModel>
    {
        public Guid ProviderId { get; set; }
    }

    public class ViewModel
    {
        public Guid ProviderId { get; set; }
        public bool GotContact { get; set; }
        public IReadOnlyCollection<string> AddressParts { get; set; }
        public string ContactName { get; set; }
        public string Website { get; set; }
        public string Email { get; set; }
        public string Telephone { get; set; }
    }

    public class Handler : IRequestHandler<Query, ViewModel>
    {
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;

        public Handler(ICosmosDbQueryDispatcher cosmosDbQueryDispatcher)
        {
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
        }

        public async Task<ViewModel> Handle(Query request, CancellationToken cancellationToken)
        {
            var provider = await _cosmosDbQueryDispatcher.ExecuteQuery(
                new GetProviderById()
                {
                    ProviderId = request.ProviderId
                });

            if (provider == null)
            {
                throw new ErrorException<ProviderDoesNotExist>(new ProviderDoesNotExist());
            }

            var contact = provider.ProviderContact
                .OrderByDescending(c => c.LastUpdated)
                .SingleOrDefault(c => c.ContactType == "P");

            if (contact == null)
            {
                return new ViewModel()
                {
                    ProviderId = request.ProviderId,
                    GotContact = false
                };
            }

            var contactName = contact.ContactPersonalDetails?.PersonGivenName != null && contact.ContactPersonalDetails?.PersonFamilyName != null ?
                string.Join(" ", contact.ContactPersonalDetails.PersonGivenName) + " " +
                    contact.ContactPersonalDetails.PersonFamilyName :
                null;

            return new ViewModel()
            {
                AddressParts = contact.ContactAddress != null ?
                    FormatAddress(contact.ContactAddress) :
                    Array.Empty<string>(),
                ContactName = contactName,
                Email = contact.ContactEmail,
                GotContact = true,
                ProviderId = provider.Id,
                Telephone = contact.ContactTelephone1,
                Website = contact.ContactWebsiteAddress != null ? UrlUtil.EnsureHttpPrefixed(contact.ContactWebsiteAddress) : null
            };

            IReadOnlyCollection<string> FormatAddress(ProviderContactAddress address)
            {
                var parts = new List<string>()
                {
                    address.SAON?.Description,
                    address.PAON?.Description,
                    address.StreetDescription,
                    address.Locality
                };

                if (address.Items != null)
                {
                    parts.AddRange(address.Items);
                }

                parts.Add(address.PostCode);

                return parts.Where(p => !string.IsNullOrEmpty(p)).ToList();
            }
        }
    }
}
