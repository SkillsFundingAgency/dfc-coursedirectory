CREATE VIEW [Pttcd].[vwProviderSearchIndex]
AS
SELECT      p.ProviderId,
            p.Ukprn,
            p.ProviderName,
            ISNULL(pc.AddressPostTown, pc.AddressItems) AS Town,
            pc.AddressPostcode AS Postcode,
            p.ProviderStatus,
            p.UkrlpProviderStatusDescription,
            p.Version, -- Timestamp (RowVersion) type, used here for cosmos HighWaterMark

            -- additional fields to skew the search index score approximately as per the old index
            COALESCE(p.CourseDirectoryName, p.ProviderName) AS CourseDirectoryName, -- as per previous cosmos-based search index definition
            p.TradingName,
            p.Alias,
            p.UpdatedOn,
            CAST(NULL AS DATETIME) AS DateOnboarded,
            CAST(NULL AS VARCHAR(255)) AS Region
FROM        [Pttcd].[Providers] p
OUTER APPLY (
    SELECT TOP 1    AddressItems,
                    NULLIF(AddressPostTown, '') AS AddressPostTown,
                    AddressPostcode
    FROM            [Pttcd].[ProviderContacts]
    WHERE           ProviderId = p.ProviderId
    AND             ContactType = 'P'
) pc
