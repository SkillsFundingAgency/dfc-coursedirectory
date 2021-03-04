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
            -- The following fields are only here to affect the scoring of results to approximate the behaviour of the old comsos based index:
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
