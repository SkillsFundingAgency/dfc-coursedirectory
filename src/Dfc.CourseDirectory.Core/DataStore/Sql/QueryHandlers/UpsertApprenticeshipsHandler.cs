using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class UpsertApprenticeshipsHandler : ISqlQueryHandler<UpsertApprenticeships, None>
    {
        public async Task<None> Execute(SqlTransaction transaction, UpsertApprenticeships query)
        {
            await UpsertApprenticeships();
            await UpsertApprenticeshipLocations();
            await UpsertApprenticeshipLocationRegions();

            return new None();

            async Task UpsertApprenticeships()
            {
                var createTableSql = @"
CREATE TABLE #Apprenticeships (
    ApprenticeshipId UNIQUEIDENTIFIER,
    ApprenticeshipStatus INT,
    CreatedOn DATETIME,
    CreatedBy NVARCHAR(MAX),
    UpdatedOn DATETIME,
    UpdatedBy NVARCHAR(MAX),
    TribalApprenticeshipId INT,
    ProviderId UNIQUEIDENTIFIER,
    ProviderUkprn INT,
    ApprenticeshipType TINYINT,
    ApprenticeshipTitle NVARCHAR(MAX),
    StandardCode INT,
    StandardVersion INT,
    FrameworkCode INT,
    FrameworkProgType INT,
    FrameworkPathwayCode INT,
    MarketingInformation NVARCHAR(MAX),
    ApprenticeshipWebsite NVARCHAR(MAX),
    ContactTelephone NVARCHAR(MAX),
    ContactEmail NVARCHAR(MAX),
    ContactWebsite NVARCHAR(MAX)
)";
                await transaction.Connection.ExecuteAsync(createTableSql, transaction: transaction);

                await BulkCopyHelper.WriteRecords(
                    query.Records.Select(a => new
                    {
                        a.ApprenticeshipId,
                        a.ApprenticeshipStatus,
                        a.CreatedOn,
                        a.CreatedBy,
                        a.UpdatedOn,
                        a.UpdatedBy,
                        a.TribalApprenticeshipId,
                        a.ProviderId,
                        a.ProviderUkprn,
                        a.ApprenticeshipType,
                        a.ApprenticeshipTitle,
                        a.StandardCode,
                        a.StandardVersion,
                        a.FrameworkCode,
                        a.FrameworkProgType,
                        a.FrameworkPathwayCode,
                        a.MarketingInformation,
                        a.ApprenticeshipWebsite,
                        a.ContactTelephone,
                        a.ContactEmail,
                        a.ContactWebsite
                    }),
                    tableName: "#Apprenticeships",
                    transaction: transaction);

                var mergeSql = @"
MERGE Pttcd.Apprenticeships AS target
USING (SELECT * FROM #Apprenticeships) AS source
ON target.ApprenticeshipId = source.ApprenticeshipId
WHEN NOT MATCHED THEN
    INSERT (
        ApprenticeshipId,
        ApprenticeshipStatus,
        CreatedOn,
        CreatedBy,
        UpdatedOn,
        UpdatedBy,
        TribalApprenticeshipId,
        ProviderId,
        ProviderUkprn,
        ApprenticeshipType,
        ApprenticeshipTitle,
        StandardCode,
        StandardVersion,
        FrameworkCode,
        FrameworkProgType,
        FrameworkPathwayCode,
        MarketingInformation,
        ApprenticeshipWebsite,
        ContactTelephone,
        ContactEmail,
        ContactWebsite
    ) VALUES (
        source.ApprenticeshipId,
        source.ApprenticeshipStatus,
        source.CreatedOn,
        source.CreatedBy,
        source.UpdatedOn,
        source.UpdatedBy,
        source.TribalApprenticeshipId,
        source.ProviderId,
        source.ProviderUkprn,
        source.ApprenticeshipType,
        source.ApprenticeshipTitle,
        source.StandardCode,
        source.StandardVersion,
        source.FrameworkCode,
        source.FrameworkProgType,
        source.FrameworkPathwayCode,
        source.MarketingInformation,
        source.ApprenticeshipWebsite,
        source.ContactTelephone,
        source.ContactEmail,
        source.ContactWebsite
    )
WHEN MATCHED THEN
    UPDATE SET
        ApprenticeshipId = source.ApprenticeshipId,
        ApprenticeshipStatus = source.ApprenticeshipStatus,
        CreatedOn = source.CreatedOn,
        CreatedBy = source.CreatedBy,
        UpdatedOn = source.UpdatedOn,
        UpdatedBy = source.UpdatedBy,
        TribalApprenticeshipId = source.TribalApprenticeshipId,
        ProviderId = source.ProviderId,
        ProviderUkprn = source.ProviderUkprn,
        ApprenticeshipType = source.ApprenticeshipType,
        ApprenticeshipTitle = source.ApprenticeshipTitle,
        StandardCode = source.StandardCode,
        StandardVersion = source.StandardVersion,
        FrameworkCode = source.FrameworkCode,
        FrameworkProgType = source.FrameworkProgType,
        FrameworkPathwayCode = source.FrameworkPathwayCode,
        MarketingInformation = source.MarketingInformation,
        ApprenticeshipWebsite = source.ApprenticeshipWebsite,
        ContactTelephone = source.ContactTelephone,
        ContactEmail = source.ContactEmail,
        ContactWebsite = source.ContactWebsite;";

                await transaction.Connection.ExecuteAsync(mergeSql, transaction: transaction);
            }

            async Task UpsertApprenticeshipLocations()
            {
                var createTableSql = @"
CREATE TABLE #ApprenticeshipLocations (
    ApprenticeshipLocationId UNIQUEIDENTIFIER,
    ApprenticeshipId UNIQUEIDENTIFIER,
    ApprenticeshipLocationStatus INT,
    CreatedOn DATETIME,
    CreatedBy NVARCHAR(MAX),
    UpdatedOn DATETIME,
    UpdatedBy NVARCHAR(MAX),
    Telephone NVARCHAR(MAX),
    VenueId UNIQUEIDENTIFIER,
    TribalApprenticeshipLocationId INT,
    [National] BIT,
    Radius INT,
    LocationType TINYINT,
    ApprenticeshipLocationType TINYINT,
    Name NVARCHAR(MAX),
    DeliveryModes TINYINT
)";
                await transaction.Connection.ExecuteAsync(createTableSql, transaction: transaction);

                await BulkCopyHelper.WriteRecords(
                    query.Records.SelectMany(a => a.Locations.Select(l => new
                    {
                        l.ApprenticeshipLocationId,
                        a.ApprenticeshipId,
                        l.ApprenticeshipLocationStatus,
                        l.CreatedOn,
                        l.CreatedBy,
                        l.UpdatedOn,
                        l.UpdatedBy,
                        l.Telephone,
                        l.VenueId,
                        l.TribalApprenticeshipLocationId,
                        l.National,
                        l.Radius,
                        l.LocationType,
                        l.ApprenticeshipLocationType,
                        l.Name,
                        DeliveryModes = MapDeliveryModes(l.DeliveryModes)
                    })),
                    tableName: "#ApprenticeshipLocations",
                    transaction: transaction);

                var mergeSql = @"
-- Remove any regions for deleted locations
DELETE alr FROM Pttcd.ApprenticeshipLocationRegions alr
JOIN Pttcd.ApprenticeshipLocations al ON alr.ApprenticeshipLocationId = al.ApprenticeshipLocationId
JOIN #Apprenticeships a ON al.ApprenticeshipId = a.ApprenticeshipId
LEFT JOIN #ApprenticeshipLocations x ON alr.ApprenticeshipLocationId = x.ApprenticeshipLocationId
WHERE x.ApprenticeshipLocationId IS NULL

MERGE Pttcd.ApprenticeshipLocations AS target
USING (SELECT * FROM #ApprenticeshipLocations) AS source
ON target.ApprenticeshipLocationId = source.ApprenticeshipLocationId
WHEN NOT MATCHED THEN
    INSERT (
        ApprenticeshipLocationId,
        ApprenticeshipId,
        ApprenticeshipLocationStatus,
        CreatedOn,
        CreatedBy,
        UpdatedOn,
        UpdatedBy,
        Telephone,
        VenueId,
        TribalApprenticeshipLocationId,
        [National],
        Radius,
        LocationType,
        ApprenticeshipLocationType,
        Name,
        DeliveryModes
    ) VALUES (
        source.ApprenticeshipLocationId,
        source.ApprenticeshipId,
        source.ApprenticeshipLocationStatus,
        source.CreatedOn,
        source.CreatedBy,
        source.UpdatedOn,
        source.UpdatedBy,
        source.Telephone,
        source.VenueId,
        source.TribalApprenticeshipLocationId,
        source.[National],
        source.Radius,
        source.LocationType,
        source.ApprenticeshipLocationType,
        source.Name,
        source.DeliveryModes
    )
WHEN MATCHED THEN
    UPDATE SET
        ApprenticeshipLocationId = source.ApprenticeshipLocationId,
        ApprenticeshipId = source.ApprenticeshipId,
        ApprenticeshipLocationStatus = source.ApprenticeshipLocationStatus,
        CreatedOn = source.CreatedOn,
        CreatedBy = source.CreatedBy,
        UpdatedOn = source.UpdatedOn,
        UpdatedBy = source.UpdatedBy,
        Telephone = source.Telephone,
        VenueId = source.VenueId,
        TribalApprenticeshipLocationId = source.TribalApprenticeshipLocationId,
        [National] = source.[National],
        Radius = source.Radius,
        LocationType = source.LocationType,
        ApprenticeshipLocationType = source.ApprenticeshipLocationType,
        Name = source.Name,
        DeliveryModes = source.DeliveryModes
WHEN NOT MATCHED BY SOURCE AND target.ApprenticeshipId IN (SELECT ApprenticeshipId FROM #Apprenticeships) THEN DELETE;";

                await transaction.Connection.ExecuteAsync(mergeSql, transaction: transaction);
            }

            async Task UpsertApprenticeshipLocationRegions()
            {
                var createTableSql = @"
CREATE TABLE #ApprenticeshipLocationRegions (
    ApprenticeshipLocationId UNIQUEIDENTIFIER,
    RegionId VARCHAR(9) COLLATE SQL_Latin1_General_CP1_CI_AS
)";
                await transaction.Connection.ExecuteAsync(createTableSql, transaction: transaction);

                await BulkCopyHelper.WriteRecords(
                    query.Records.SelectMany(a => a.Locations.SelectMany(al => al.Regions.Select(r => new
                    {
                        al.ApprenticeshipLocationId,
                        RegionId = r
                    }))),
                    tableName: "#ApprenticeshipLocationRegions",
                    transaction);

                var mergeSql = @"
MERGE Pttcd.ApprenticeshipLocationRegions AS target
USING (
    SELECT ApprenticeshipLocationId, RegionId FROM #ApprenticeshipLocationRegions
) AS source
ON target.ApprenticeshipLocationId = source.ApprenticeshipLocationId AND target.RegionId = source.RegionId
WHEN NOT MATCHED THEN
    INSERT (ApprenticeshipLocationId, RegionId) VALUES (source.ApprenticeshipLocationId, source.RegionId)
WHEN NOT MATCHED BY SOURCE AND target.ApprenticeshipLocationId IN (SELECT ApprenticeshipLocationId FROM #ApprenticeshipLocations) THEN DELETE;";

                await transaction.Connection.ExecuteAsync(mergeSql, transaction: transaction);
            }
            
            static int MapDeliveryModes(IEnumerable<ApprenticeshipDeliveryMode> deliveryModes) =>
                deliveryModes
                    .Select(dm => dm switch
                    {
                        ApprenticeshipDeliveryMode.EmployerAddress => 1,
                        ApprenticeshipDeliveryMode.DayRelease => 2,
                        ApprenticeshipDeliveryMode.BlockRelease => 4,
                        (ApprenticeshipDeliveryMode)4 => 5,
                        _ => throw new NotSupportedException($"Unknown delivery mode: '{dm}'.")
                    })
                    .Aggregate(seed: 0, (current, v) => current | v);
        }
    }
}
