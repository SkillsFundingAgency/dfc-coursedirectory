﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.WebV2.DataStore.Sql;
using Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries;
using Dfc.CourseDirectory.WebV2.Models;
using Dfc.CourseDirectory.WebV2.Security;
using MediatR;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.WebV2.Features.ApprenticeshipQA.ProviderApprenticeshipQAInfoPanel
{
    using QueryResponse = OneOf<NotFound, ViewModel>;

    public class Query : IRequest<QueryResponse>
    {
        public Guid ProviderId { get; set; }
    }

    public class ViewModel
    {
        public Guid ProviderId { get; set; }
        public string ProviderName { get; set; }
        public int Ukprn { get; set; }
        public IReadOnlyCollection<string> AddressParts { get; set; }
        public string ContactName { get; set; }
        public string Website { get; set; }
        public string Email { get; set; }
        public string Telephone { get; set; }
        public UserInfo LastSignedIn { get; set; }
        public DateTime? LastSignedInDate { get; set; }
        public UserInfo LastAssessedBy { get; set; }
        public DateTime? LastAssessedOn { get; set; }
    }

    public class Handler : IRequestHandler<Query, QueryResponse>
    {
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly IClock _clock;
        private readonly ICurrentUserProvider _currentUserProvider;

        public Handler(
            ISqlQueryDispatcher sqlQueryDispatcher,
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher,
            IClock clock,
            ICurrentUserProvider currentUserProvider)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
            _clock = clock;
            _currentUserProvider = currentUserProvider;
        }

        public async Task<QueryResponse> Handle(Query request, CancellationToken cancellationToken)
        {
            var provider = await _cosmosDbQueryDispatcher.ExecuteQuery(
                new GetProviderById()
                {
                    ProviderId = request.ProviderId
                });

            if (provider == null)
            {
                return new NotFound();
            }

            var lastAssessment = await _sqlQueryDispatcher.ExecuteQuery(
                new GetLatestApprenticeshipQASubmissionForProvider()
                {
                    ProviderId = request.ProviderId
                });

            var lastAssessedBy = lastAssessment.AsT1?.LastAssessedBy ?? _currentUserProvider.GetCurrentUser();
            var lastAssessedOn = lastAssessment.AsT1?.LastAssessedOn ?? _clock.UtcNow;

            var contact = provider.ProviderContact
                .OrderByDescending(c => c.LastUpdated)
                .SingleOrDefault(c => c.ContactType == "L");

            var contactName = contact?.ContactPersonalDetails.PersonGivenName != null && contact?.ContactPersonalDetails.PersonFamilyName != null ?
                string.Join(" ", contact.ContactPersonalDetails.PersonGivenName) + " " +
                    contact.ContactPersonalDetails.PersonFamilyName :
                null;

            var latestSignIn = await _sqlQueryDispatcher.ExecuteQuery(
                new GetLatestUserSignInForProvider()
                {
                    ProviderId = request.ProviderId
                });

            return new ViewModel()
            {
                AddressParts = contact?.ContactAddress != null ?
                    FormatAddress(contact.ContactAddress) :
                    Array.Empty<string>(),
                ContactName = contactName,
                Email = contact?.ContactEmail,
                LastAssessedBy = lastAssessedBy,
                LastAssessedOn = lastAssessedOn,
                LastSignedIn = latestSignIn.Match(_ => null, signIn => signIn.User),
                LastSignedInDate = latestSignIn.Match(_ => (DateTime?)null, signIn => signIn.SignedInUtc),
                ProviderId = provider.Id,
                ProviderName = provider.ProviderName,
                Telephone = contact?.ContactTelephone1,
                Ukprn = int.Parse(provider.UnitedKingdomProviderReferenceNumber),
                Website = contact?.ContactWebsiteAddress != null ? UrlUtil.EnsureHttpPrefixed(contact.ContactWebsiteAddress) : null
            };

            IReadOnlyCollection<string> FormatAddress(ContactAddress address)
            {
                var parts = new List<string>()
                {
                    address.SAON?.Description,
                    address.PAON?.Description,
                    address.StreetDescription,
                    address.Locality
                };
                parts.AddRange(address.Items);
                parts.Add(address.PostCode);

                return parts.Where(p => !string.IsNullOrEmpty(p)).ToList();
            }
        }
    }
}
