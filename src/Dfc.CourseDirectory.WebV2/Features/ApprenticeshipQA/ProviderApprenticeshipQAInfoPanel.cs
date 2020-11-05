using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using Dfc.CourseDirectory.WebV2.Security;
using MediatR;

namespace Dfc.CourseDirectory.WebV2.Features.ApprenticeshipQA.ProviderApprenticeshipQAInfoPanel
{
    public class Query : IRequest<ViewModel>
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

    public class Handler : IRequestHandler<Query, ViewModel>
    {
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;

        public Handler(
            ISqlQueryDispatcher sqlQueryDispatcher,
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
        }

        public async Task<ViewModel> Handle(Query request, CancellationToken cancellationToken)
        {
            var provider = await _cosmosDbQueryDispatcher.ExecuteQuery(
                new Core.DataStore.CosmosDb.Queries.GetProviderById()
                {
                    ProviderId = request.ProviderId
                });

            if (provider == null)
            {
                throw new InvalidStateException(InvalidStateReason.ProviderDoesNotExist);
            }

            var lastSubmission = await _sqlQueryDispatcher.ExecuteQuery(
                new GetLatestApprenticeshipQASubmissionForProvider()
                {
                    ProviderId = request.ProviderId
                });

            var lastAssessedBy = (lastSubmission.Value as ApprenticeshipQASubmission)?.LastAssessedBy;
            var lastAssessedOn = (lastSubmission.Value as ApprenticeshipQASubmission)?.LastAssessedOn;

            var contact = provider.ProviderContact
                .OrderByDescending(c => c.LastUpdated)
                .SingleOrDefault(c => c.ContactType == "P");

            var contactName = contact?.ContactPersonalDetails?.PersonGivenName != null && contact?.ContactPersonalDetails?.PersonFamilyName != null ?
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
