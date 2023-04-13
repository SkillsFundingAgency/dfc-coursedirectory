using System;
using Dapper;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Dfc.CourseDirectory.Functions;
using Microsoft.Azure.WebJobs;

public class UpdateFindACourseIndexExpiredFreeCourses
{
    private readonly ISqlQueryDispatcherFactory _sqlQueryDispatcherFactory;

    public UpdateFindACourseIndexExpiredFreeCourses(ISqlQueryDispatcherFactory sqlQueryDispatcherFactory)
	{
        _sqlQueryDispatcherFactory = sqlQueryDispatcherFactory;
    }

    [FunctionName(nameof(UpdateFindACourseIndexExpiredFreeCourses))]
    [Singleton]
    public async Task RunAsync([TimerTrigger("0 0 5 * * *")] TimerInfo myTimer)
    {
        using (var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher())
        {
            var sql = @"
DECLARE @ExpiredFreeLARS TABLE(LearnAimRef NVARCHAR(50));
    INSERT INTO @ExpiredFreeLARS
    SELECT LearnAimRef
    FROM [LARS].[LearningDeliveryCategory]
    WHERE EffectiveTo < GETDATE() AND (CategoryRef = 45 or CategoryRef = 46)

DECLARE @FindACourseIndexRowsToBeUpdated TABLE(Id VARCHAR(46), LasySynced DATETIME, OfferingType INT, CourseId UNIQUEIDENTIFIER, CourseRunId UNIQUEIDENTIFIER, TLevelId UNIQUEIDENTIFIER,
                                    TLevelLocationId UNIQUEIDENTIFIER, Live BIT, VersionTemp DATETIME, UpdatedOn DATETIME, ProviderId UNIQUEIDENTIFIER, ProviderDisplayName NVARCHAR(max),
                                    ProviderUkprn INT, QualificationCourseTitle NVARCHAR(max), LearnAimRef VARCHAR(50), NotionalNVQLevelv2 NVARCHAR(max), CourseDescription NVARCHAR(max),
                                    CourseName NVARCHAR(max), DeliveryMode TINYINT, FlexibleStartDate BIT, StartDate DATE, Cost MONEY, CostDescription NVARCHAR(max), 
                                    DurationUnit TINYINT, DurationValue INT, StudyMode TINYINT, AttendencePattern TINYINT, NationalTemp BIT, VenueName NVARCHAR(max), VenueAddress NVARCHAR(max),
                                    VenueTown NVARCHAR(max), Position GEOGRAPHY, RegionName NVARCHAR(100), ScoreBoost FLOAT, VenueId UNIQUEIDENTIFIER, CampaignCodes NVARCHAR(max),
                                    CourseDataIsHtmlEncoded BIT, CourseRunDatalsHtmlEncoded BIT);

INSERT INTO @FindACourseIndexRowsToBeUpdated
SELECT * 
FROM [Pttcd].[FindACourseIndex] f 
WHERE EXISTS(SELECT * FROM @ExpiredFreeLARS e WHERE e.LearnAimRef = f.LearnAimRef) AND CampaignCodes IS NOT NULL

UPDATE @FindACourseIndexRowsToBeUpdated SET CampaignCodes = NULL, UpdatedOn = GETDATE() WHERE CampaignCodes = '[""LEVEL3_FREE""]' 
UPDATE @FindACourseIndexRowsToBeUpdated SET CampaignCodes = REPLACE(CampaignCodes, '""LEVEL3_FREE"",', ''), UpdatedOn = GETDATE() WHERE CampaignCodes LIKE '%""LEVEL3_FREE""%'
UPDATE @FindACourseIndexRowsToBeUpdated SET CampaignCodes = REPLACE(CampaignCodes, ',""LEVEL3_FREE""', ''), UpdatedOn = GETDATE() WHERE CampaignCodes LIKE '%""LEVEL3_FREE""%'
UPDATE @FindACourseIndexRowsToBeUpdated SET CampaignCodes = REPLACE(CampaignCodes, '""LEVEL3_FREE"", ', ''), UpdatedOn = GETDATE() WHERE CampaignCodes LIKE '%""LEVEL3_FREE""%'
UPDATE @FindACourseIndexRowsToBeUpdated SET CampaignCodes = REPLACE(CampaignCodes, ', ""LEVEL3_FREE""', ''), UpdatedOn = GETDATE() WHERE CampaignCodes LIKE '%""LEVEL3_FREE""%'

UPDATE fac SET CampaignCodes = facToUpdate.CampaignCodes, UpdatedOn = facToUpdate.UpdatedOn FROM [Pttcd].[FindACourseIndex] fac INNER JOIN @FindACourseIndexRowsToBeUpdated facToUpdate ON fac.Id = facToUpdate.Id
";
            await dispatcher.Transaction.Connection.ExecuteAsync(sql, transaction: dispatcher.Transaction);

            await dispatcher.Commit();
        }
    }
}
