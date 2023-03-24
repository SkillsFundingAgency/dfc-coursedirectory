using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class UpdateFindACourseIndexForExpiredFreeCourseFundingHandler : ISqlQueryHandler<UpdateFindACourseIndexForExpiredFreeCourseFunding, Success>
    {
        public async Task<Success> Execute(SqlTransaction transaction, UpdateFindACourseIndexForExpiredFreeCourseFunding query)
        {
            var sql = $@"
                        DECLARE	@today DATETIME = GETDATE()
DECLARE @FundingEndDateAsChar NVARCHAR(50)
DECLARE @FundingEndDate DATE

SET @FundingEndDateAsChar =
(SELECT EffectiveTo
FROM  [LARS].[LearningDeliveryCategory]
WHERE LearnAimRef = @LearnAimRefs AND (CategoryRef = 45 OR CategoryRef = 46))

IF @FundingEndDateAsChar IS NOT NULL
BEGIN
    SET @FundingEndDate = CONVERT(DATE, @FundingEndDateAsChar)
    
    IF @FundingEndDate < @today
        DECLARE @NumberOfCoursesWithLearnAimRef INT =
        (SELECT COUNT(*)
        FROM [Pttcd].[FindACourseIndex]
        WHERE LearnAimRef = @LearnAimRefs)
        
        DECLARE @TempFindACourseIndexWithRowNumbers TABLE(Id VARCHAR(46), RowNumber INT);
        INSERT INTO @TempFindACourseIndexWithRowNumbers
        SELECT Id, ROW_NUMBER() OVER (ORDER BY Id ASC) AS RowNumber
        FROM [Pttcd].[FindACourseIndex]
        WHERE LearnAimRef = @LearnAimRefs

        DECLARE @i INT = 0
        WHILE @i < @NumberOfCoursesWithLearnAimRef
        BEGIN
            SET @i = @i + 1

            DECLARE @IdThisRow VARCHAR(46) =
            (SELECT Id
            FROM @TempFindACourseIndexWithRowNumbers
            WHERE RowNumber = @i)

            DECLARE @CampaignCodesThisRow NVARCHAR(max) = 
            (SELECT CampaignCodes
            FROM [Pttcd].[FindACourseIndex]
            WHERE Id = @IdThisRow)

            IF @CampaignCodesThisRow = '[""LEVEL3_FREE""]'
            BEGIN
                UPDATE [Pttcd].[FindACourseIndex] SET CampaignCodes = NULL, UpdatedOn = @today WHERE Id = @IdThisRow AND LearnAimRef = @LearnAimRefs
            END
            ELSE
            BEGIN
                IF @CampaignCodesThisRow IS NOT NULL
                BEGIN
                    DECLARE @NewCampaignCodesThisRow NVARCHAR(max) = REPLACE(@CampaignCodesThisRow, ', ""LEVEL3_FREE""', '')
                    SET @NewCampaignCodesThisRow = REPLACE(@CampaignCodesThisRow, '""LEVEL3_FREE"", ', '') --in case LEVEL3_FREE is the first string
                    UPDATE [Pttcd].[FindACourseIndex] SET CampaignCodes = @NewCampaignCodesThisRow, UpdatedOn = @today WHERE Id = @IdThisRow AND LearnAimRef = @LearnAimRefs
                END
            END
        END
END";

            var paramz = new
            {
                LearnAimRefs = TvpHelper.CreateGuidIdTable(query.LearnAimRefs)
            };

            await transaction.Connection.ExecuteAsync(sql, paramz, transaction);

            return new Success();
        }
    }
}
