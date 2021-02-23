CREATE VIEW [Pttcd].[vwProviderSearchIndex]
AS
SELECT      p.ProviderId,
            p.Ukprn,
            p.ProviderName,
            p.ProviderStatus,
            p.UkrlpProviderStatusDescription,
            pc.AddressPostcode Postcode,
            ISNULL(pc.AddressPostTown, pc.AddressItems) Town,
            p.Version -- Timestamp (RowVersion) type, used here for cosmos HighWaterMark
FROM        [Pttcd].[Providers] p
OUTER APPLY (
    SELECT TOP 1    AddressItems,
                    NULLIF(AddressPostTown, '') AddressPostTown,
                    AddressPostcode
    FROM            [Pttcd].[ProviderContacts]
    WHERE           ProviderId = p.ProviderId
    AND             ContactType = 'P'
) pc
