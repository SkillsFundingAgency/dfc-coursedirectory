create
    --or alter -- not supported by dacpac
    view Pttcd.vwProviderSearchIndex
as
select
    p.ProviderId,
    p.Ukprn,
    p.ProviderName,
    p.ProviderStatus,
    p.UkrlpProviderStatusDescription,
    p_contact.AddressPostcode Postcode,
    p_contact.AddressItems Town, -- for approximate compatibility, the old index was reading Items[0] which has been squashed into this field in sql
    p.Version -- Timestamp (RowVersion) type, used here for cosmos HighWaterMark
from Pttcd.Providers p
outer apply (
    select top 1 pc.ProviderId, pc.ContactType, pc.AddressItems, pc.AddressPostTown, pc.AddressPostcode
    from Pttcd.ProviderContacts pc
    where pc.ProviderId = p.ProviderId and pc.ContactType = 'P'
) p_contact
