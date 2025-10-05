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
                SET UploadStatus = {(int)UploadStatus.Published}, PublishedOn = @PublishedOn
                WHERE ProviderUploadId = @ProviderUploadId

                IF @@ROWCOUNT = 0
                BEGIN
                    SELECT 0 AS Status
                    RETURN
                END


                -- Create new providers from the Provider Upload's rows

                ;WITH ProviderCte AS (
                    SELECT
                        ProviderId,
                        ProviderStatus,
                        ProviderType,
                        Ukprn,
                        ProviderName,
                        LegalName,
                        ROW_NUMBER() OVER (PARTITION BY ProviderId ORDER BY RowNumber) AS GroupRowNumber
                    FROM Pttcd.ProviderUploadRows
                    WHERE ProviderUploadId = @ProviderUploadId
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
                         )
                SELECT
                    ProviderId,
                    NULL,
                    Ukprn,
                    {(int)CourseStatus.Live},
                    ProviderType,
                    ProviderName,
                    NULL,
                    NULL,
                    NULL,
                    TradingName,
                    NULL,
                    @PublishedOn,
                    @PublishedByUserId,
                    NULL,
                    NULL,
                    NULL,
                    NULL,
                    NULL,
                    NULL                    
                FROM ProvidersCte
                WHERE GroupRowNumber = 1

                SELECT 1 AS Status

                SELECT CourseRunId FROM Pttcd.CourseUploadRows
                WHERE CourseUploadId = @CourseUploadId
                AND CourseUploadRowStatus = {(int)UploadRowStatus.Default}
                ";

            var paramz = new
            {
                query.ProviderUploadId,
                query.OnboardedOn,
                PublishedByUserId = query.OnboardedBy.UserId
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
