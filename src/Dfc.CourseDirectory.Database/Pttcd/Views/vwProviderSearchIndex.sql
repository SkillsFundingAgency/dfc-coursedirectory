CREATE VIEW [Pttcd].[vwProviderSearchIndex]
AS
SELECT      p.ProviderId,
            p.Ukprn,
            p.ProviderName,
            ISNULL(pc.AddressPostTown, pc.AddressItems) Town,
            pc.AddressPostcode Postcode,
            p.ProviderStatus,
            p.UkrlpProviderStatusDescription,
            p.Version, -- Timestamp (RowVersion) type, used here for cosmos HighWaterMark

            -- additional fields to skew the search index score approximately as per the old index
            coalesce(p.CourseDirectoryName,p.ProviderName) CourseDirectoryName, -- as per previous cosmos-based search index definition
            p.TradingName,
            p.Alias
FROM        [Pttcd].[Providers] p
OUTER APPLY (
    SELECT TOP 1    AddressItems,
                    NULLIF(AddressPostTown, '') AddressPostTown,
                    AddressPostcode
    FROM            [Pttcd].[ProviderContacts]
    WHERE           ProviderId = p.ProviderId
    AND             ContactType = 'P'
) pc
