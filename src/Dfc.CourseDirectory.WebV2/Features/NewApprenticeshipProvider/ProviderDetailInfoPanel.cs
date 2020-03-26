using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.Behaviors.Errors;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Models;
using Dfc.CourseDirectory.WebV2.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.WebV2.DataStore.Sql;
using Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries;
using Dfc.CourseDirectory.WebV2.Models;
using Dfc.CourseDirectory.WebV2.Security;
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
        public IReadOnlyCollection<string> AddressParts { get; set; }
        public string ContactName { get; set; }
        public string Website { get; set; }
        public string Email { get; set; }
        public string Telephone { get; set; }
    }

    public class Handler : IRequestHandler<Query, ViewModel>
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

            var contactName = contact?.ContactPersonalDetails.PersonGivenName != null && contact?.ContactPersonalDetails.PersonFamilyName != null ?
                string.Join(" ", contact.ContactPersonalDetails.PersonGivenName) + " " +
                    contact.ContactPersonalDetails.PersonFamilyName :
                null;

            return new ViewModel()
            {
                AddressParts = contact?.ContactAddress != null ?
                    FormatAddress(contact.ContactAddress) :
                    Array.Empty<string>(),
                ContactName = contactName,
                Email = contact?.ContactEmail,
                ProviderId = provider.Id,
                Telephone = contact?.ContactTelephone1,
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
