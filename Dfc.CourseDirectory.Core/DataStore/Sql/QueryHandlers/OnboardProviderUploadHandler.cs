using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class OnboardProviderUploadHandler : ISqlQueryHandler<OnboardProviderUpload, OneOf<NotFound, OnboardProviderUploadResult>>
    {
        public async Task<OneOf<NotFound, OnboardProviderUploadResult>> Execute(SqlTransaction transaction, OnboardProviderUpload query)
        {
            var sql = $@"
                UPDATE Pttcd.ProviderUploads
                SET UploadStatus = {(int)UploadStatus.Published}, PublishedOn = @OnboardedOn
                WHERE ProviderUploadId = @ProviderUploadId

                IF @@ROWCOUNT = 0
                BEGIN
                    SELECT 0 AS Status
                    RETURN
                END
                DECLARE @UploadedBy UNIQUEIDENTIFIER

                SELECT @UploadedBy = CreatedByUserId from pttcd.providerUploads 
                WHERE ProviderUploadId = @ProviderUploadId

                --Update existing provider
                  Update
                   p
                  Set 
                 P.UploadResult = (CASE
                    WHEN p.providerstatus <> pur.providerstatus ANd p.providertype <> pur.providertype THEN {(int)ProviderUploadResult.ProviderStatusAndTypeUpdated}
                    WHEN p.providerstatus = pur.providerstatus ANd p.providertype <> pur.providertype THEN {(int)ProviderUploadResult.ProviderTypeUpdated}
                    WHEN p.providerstatus <> pur.providerstatus ANd p.providertype = pur.providertype THEN {(int)ProviderUploadResult.ProviderStatusUpdated}
                    ELSE P.UploadResult
                END) ,
                P.ProviderUploadId = (CASE
                    WHEN p.providerstatus <> pur.providerstatus ANd p.providertype <> pur.providertype THEN @ProviderUploadId
                    WHEN p.providerstatus = pur.providerstatus ANd p.providertype <> pur.providertype THEN @ProviderUploadId
                    WHEN p.providerstatus <> pur.providerstatus ANd p.providertype = pur.providertype THEN @ProviderUploadId
                    ELSE P.ProviderUploadId
                END) ,
                  p.ProviderStatus = pur.ProviderStatus,
                  p.ProviderType =  p.ProviderType | pur.ProviderType,
                  p.UpdatedOn = (CASE
                    WHEN p.providerstatus <> pur.providerstatus ANd p.providertype <> pur.providertype THEN @OnboardedOn
                    WHEN p.providerstatus = pur.providerstatus ANd p.providertype <> pur.providertype THEN @OnboardedOn
                    WHEN p.providerstatus <> pur.providerstatus ANd p.providertype = pur.providertype THEN @OnboardedOn
                    ELSE P.UpdatedOn
                END) 
                  from Pttcd.Providers p
                  Inner Join [Pttcd].[ProviderUploadRows] pur on p.Ukprn = pur.Ukprn

                  Where pur.provideruploadId = @ProviderUploadId

                -- Create new providers from the Provider Upload's rows

                ;WITH ProvidersCte AS (
                    SELECT
                        ProviderId,
                        ProviderStatus,
                        ProviderType,
                        Ukprn,
                        ProviderName,
                        TradingName,
                        ROW_NUMBER() OVER (PARTITION BY ProviderId ORDER BY RowNumber) AS GroupRowNumber
                    FROM Pttcd.ProviderUploadRows
                    WHERE ProviderUploadId = @ProviderUploadId
                    AND Ukprn not in (Select Ukprn from Pttcd.Providers  WHERE Ukprn is not NULL)
                    AND ProviderUploadRowStatus = {(int)UploadRowStatus.Default}
                )
                INSERT INTO Pttcd.Providers (
                     [ProviderId]
                    ,[LastSyncedFromCosmos]
                    ,[Ukprn]
                    ,[ProviderStatus]
                    ,[ProviderType]
                    ,[ProviderName]
                    ,[UkrlpProviderStatusDescription]
                    ,[MarketingInformation]
                    ,[CourseDirectoryName]
                    ,[TradingName]
                    ,[Alias]
                    ,[UpdatedOn]
                    ,[UpdatedBy]
                    ,[DisplayNameSource]
                    ,[TribalProviderId]
                    ,[BulkUploadInProgress]
                    ,[BulkUploadPublishInProgress]
                    ,[BulkUploadStartedDateTime]
                    ,[BulkUploadTotalRowCount]
                    ,[UploadResult]
                    ,[ProviderUploadId]
                         )
                SELECT
                    ProviderId,
                    NULL,
                    Ukprn,
                    {(int)ProviderStatus.Onboarded},
                    ProviderType,
                    ProviderName,
                    'Active',
                    NULL,
                    NULL,
                    TradingName,
                    TradingName,
                    @OnboardedOn,
                    @UploadedBy,
                    0,
                    NULL,
                    NULL,
                    NULL,
                    NULL,
                    NULL,
                   {(int)ProviderUploadResult.NewProvider},
                   @ProviderUploadId
                FROM ProvidersCte
                WHERE GroupRowNumber = 1

                SELECT 1 AS Status

                SELECT ProviderId FROM Pttcd.ProviderUploadRows
                WHERE ProviderUploadId = @ProviderUploadId
                AND ProviderUploadRowStatus = {(int)UploadRowStatus.Default}
                ";

            var paramz = new
            {
                query.ProviderUploadId,
                query.OnboardedOn,
            };

            using var reader = await transaction.Connection.QueryMultipleAsync(sql, paramz, transaction);

            var status = await reader.ReadSingleAsync<int>();

            if (status == 1)
            {
                var onboardedProviderRunIds = (await reader.ReadAsync<Guid>()).AsList();

                return new OnboardProviderUploadResult()
                {
                    OnboardedProviderIds = onboardedProviderRunIds
                };
            }
            else
            {
                return new NotFound();
            }
        }
    }
}
