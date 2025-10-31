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
    public class InactiveProviderUploadHandler : ISqlQueryHandler<InactiveProviderUpload, OneOf<NotFound, OnboardProviderUploadResult>>
    {
        public async Task<OneOf<NotFound, OnboardProviderUploadResult>> Execute(SqlTransaction transaction, InactiveProviderUpload query)
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

                UPDATE prov set prov.PIMSORGStatus =  pu.PIMSORGStatus , prov.PIMSORGStatusDate = pu.PIMSORGStatusDate

                FROM [Pttcd].[Providers] as prov
                INNER JOIN [Pttcd].ProviderUploadRows as pu on prov.UKPRN = pu.UKPRN 
                Where prov.ProviderUploadId = @ProviderUploadId

                SELECT 1 AS Status

                SELECT ProviderId FROM Pttcd.ProviderUploadRows
                WHERE ProviderUploadId = @ProviderUploadId
                AND ProviderUploadRowStatus = {(int)UploadRowStatus.Default}
                ";

            var paramz = new
            {
                query.ProviderUploadId,
                query.UpdatedOn,
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
