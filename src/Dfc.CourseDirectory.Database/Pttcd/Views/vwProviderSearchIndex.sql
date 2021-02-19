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
    p_contact.AddressPostTown Town,
    p.Version -- Timestamp (RowVersion) type, used here for cosmos HighWaterMark
from Pttcd.Providers p
left outer join (
        select top 1 pc.ProviderId, pc.ContactType, pc.AddressItems, pc.AddressPostTown, pc.AddressPostcode
        from Pttcd.ProviderContacts pc
        where pc.ContactType = 'P'
    ) p_contact on p_contact.ProviderId = p.ProviderId
