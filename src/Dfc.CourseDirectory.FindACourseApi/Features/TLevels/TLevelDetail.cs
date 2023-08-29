using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using MediatR;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.FindACourseApi.Features.TLevels.TLevelDetail
{
    public class Query : IRequest<OneOf<NotFound, TLevelDetailViewModel>>
    {
        public Guid TLevelId { get; set; }
    }

    public class Handler : IRequestHandler<Query, OneOf<NotFound, TLevelDetailViewModel>>
    {
        private readonly ISqlQueryDispatcher _sqlQueryDispatcher;

        public Handler(ISqlQueryDispatcher sqlQueryDispatcher)
        {
            _sqlQueryDispatcher = sqlQueryDispatcher;
        }

        public async Task<OneOf<NotFound, TLevelDetailViewModel>> Handle(Query request, CancellationToken cancellationToken)
        {
            var tLevel = await _sqlQueryDispatcher.ExecuteQuery(new GetTLevel() { TLevelId = request.TLevelId });

            if (tLevel == null)
            {
                return new NotFound();
            }

           

            var getSqlProvider = _sqlQueryDispatcher.ExecuteQuery(
                new Core.DataStore.Sql.Queries.GetProviderById { ProviderId = tLevel.ProviderId });

            var getSqlProviderContact = _sqlQueryDispatcher.ExecuteQuery(
               new Core.DataStore.Sql.Queries.GetProviderById { ProviderId = tLevel.ProviderId });

            var getVenues = _sqlQueryDispatcher.ExecuteQuery(
                new GetVenuesByIds() { VenueIds = tLevel.Locations.Select(l => l.VenueId) });

            await Task.WhenAll(getSqlProvider, getVenues);

            var sqlProvider = await getSqlProvider;
            var venues = await getVenues;


            var providerContact = sqlProvider.ProviderContact
                .SingleOrDefault(c => c.ContactType == "P");

            return new TLevelDetailViewModel()
            {
                TLevelId = tLevel.TLevelId,
                TLevelDefinitionId = tLevel.TLevelDefinition.TLevelDefinitionId,
                Qualification = new QualificationViewModel()
                {
                    FrameworkCode = tLevel.TLevelDefinition.FrameworkCode,
                    ProgType = tLevel.TLevelDefinition.ProgType,
                    QualificationLevel = tLevel.TLevelDefinition.QualificationLevel.ToString(),
                    TLevelName = HtmlEncode(tLevel.TLevelDefinition.Name)
                },
                Provider = new ProviderViewModel()
                {
                    ProviderName = sqlProvider.DisplayName,
                    Ukprn = sqlProvider.Ukprn.ToString(),
                    AddressLine1 = HtmlEncode(
                        ViewModelFormatting.ConcatAddressLines(
                            providerContact?.ContactAddress?.SAON?.Description,
                            providerContact?.ContactAddress?.PAON?.Description,
                            providerContact?.ContactAddress?.StreetDescription)),
                    AddressLine2 = HtmlEncode(providerContact?.ContactAddress?.Locality),
                    Town = HtmlEncode(providerContact?.ContactAddress?.PostTown ?? providerContact?.ContactAddress?.Items?.ElementAtOrDefault(0)),
                    Postcode = providerContact?.ContactAddress?.PostCode,
                    County = HtmlEncode(providerContact?.ContactAddress?.County ?? providerContact?.ContactAddress?.Items?.ElementAtOrDefault(1)),
                    Email = providerContact?.ContactEmail,
                    Telephone = providerContact?.ContactTelephone1,
                    Fax = providerContact?.ContactFax,
                    Website = ViewModelFormatting.EnsureHttpPrefixed(providerContact?.ContactWebsiteAddress),
                    LearnerSatisfaction = sqlProvider?.LearnerSatisfaction,
                    EmployerSatisfaction = sqlProvider?.EmployerSatisfaction
                },
                WhoFor = HtmlEncode(tLevel.WhoFor),
                EntryRequirements = HtmlEncode(tLevel.EntryRequirements),
                WhatYoullLearn = HtmlEncode(tLevel.WhatYoullLearn),
                HowYoullLearn = HtmlEncode(tLevel.HowYoullLearn),
                HowYoullBeAssessed = HtmlEncode(tLevel.HowYoullBeAssessed),
                WhatYouCanDoNext = HtmlEncode(tLevel.WhatYouCanDoNext),
                Website = ViewModelFormatting.EnsureHttpPrefixed(tLevel.Website),
                StartDate = tLevel.StartDate,
                Locations = tLevel.Locations
                    .Select(l => (Venue: venues[l.VenueId], Location: l))
                    .Select(t => new TLevelLocationViewModel()
                    {
                        TLevelLocationId = t.Location.TLevelLocationId,
                        VenueName = HtmlEncode(t.Venue.VenueName),
                        AddressLine1 = HtmlEncode(t.Venue.AddressLine1),
                        AddressLine2 = HtmlEncode(t.Venue.AddressLine2),
                        Town = HtmlEncode(t.Venue.Town),
                        County = HtmlEncode(t.Venue.County),
                        Postcode = HtmlEncode(t.Venue.Postcode),
                        Telephone = t.Venue.Telephone,
                        Email = t.Venue.Email,
                        Website = ViewModelFormatting.EnsureHttpPrefixed(t.Venue.Website),
                        Latitude = Convert.ToDecimal(t.Venue.Latitude),
                        Longitude = Convert.ToDecimal(t.Venue.Longitude)
                    })
                    .ToArray()
            };

            static string HtmlEncode(string value) => System.Net.WebUtility.HtmlEncode(value);
        }
    }
}
