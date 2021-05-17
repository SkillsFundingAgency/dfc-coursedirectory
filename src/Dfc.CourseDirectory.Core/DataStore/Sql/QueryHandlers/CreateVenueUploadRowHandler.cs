using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class CreateVenueUploadRowHandler : ISqlQueryHandler<CreateVenueUploadRow, int>
    {
        public Task<int> Execute(SqlTransaction transaction, CreateVenueUploadRow query)
        {
            var sql = $@"
INSERT INTO [Pttcd].[VenueUploadRows]
           ([VenueUploadId]
           ,[RowNumber]
           ,[VenueUploadRowStatus]
           ,[IsValid]
           ,[Errors]
           ,[LastUpdated]
           ,[LastValidated]
           ,[ProviderVenueRef]
           ,[VenueName]
           ,[AddressLine1]
           ,[AddressLine2]
           ,[Town]
           ,[County]
           ,[Postcode]
           ,[Email]
           ,[Telephone]
           ,[Website]
           ,[VenueId]
           ,[OutsideOfEngland]
           ,[IsSupplementary])
        VALUES (
            @VenueUploadId,
            @RowNumber,
            @VenueUploadRowStatus,
            @IsValid,
            @Errors,
            @LastUpdated,
            @LastValidated,
            @ProviderVenueRef,
            @VenueName,
            @AddressLine1,
            @AddressLine2,
            @Town,
            @County,
            @Postcode,
            @Email,
            @Telephone,
            @Website,
            @VenueId,
            @OutsideOfEngland,
            @IsSupplementary
        )

        SELECT SCOPE_IDENTITY() VenueUploadRowId";

            var paramz = new
            {
                query.VenueUploadId,
                query.RowNumber,
                query.VenueUploadRowStatus,
                query.IsValid,
                query.Errors,
                query.LastUpdated,
                query.LastValidated,
                query.ProviderVenueRef,
                query.VenueName,
                query.AddressLine1,
                query.AddressLine2,
                query.Town,
                query.County,
                query.Postcode,
                query.Email,
                query.Telephone,
                query.Website,
                query.VenueId,
                query.OutsideOfEngland,
                query.IsSupplementary
            };

            //await transaction.Connection.ExecuteAsync(sql, paramz, transaction);
            

            return transaction.Connection.QuerySingleOrDefaultAsync<int>(sql, paramz, transaction);

            //return new int();
        }
    }
}
