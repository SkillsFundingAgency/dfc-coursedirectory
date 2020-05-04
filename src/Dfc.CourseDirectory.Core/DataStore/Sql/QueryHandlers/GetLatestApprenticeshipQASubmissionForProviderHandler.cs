using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.Models;
using OneOf;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetLatestApprenticeshipQASubmissionForProviderHandler
        : ISqlQueryHandler<GetLatestApprenticeshipQASubmissionForProvider, OneOf<None, ApprenticeshipQASubmission>>
    {
        public async Task<OneOf<None, ApprenticeshipQASubmission>> Execute(
            SqlTransaction transaction,
            GetLatestApprenticeshipQASubmissionForProvider query)
        {
            var sql = @"
DECLARE @LatestApprenticeshipQASubmissionId INT

SELECT TOP 1 @LatestApprenticeshipQASubmissionId = ApprenticeshipQASubmissionId
FROM Pttcd.ApprenticeshipQASubmissions s
WHERE s.ProviderId = @ProviderId
ORDER BY s.SubmittedOn DESC

SELECT TOP 1
    s.ApprenticeshipQASubmissionId,
    s.ProviderId,
    s.SubmittedOn,
    s.ProviderMarketingInformation,
    s.Passed,
    s.ProviderAssessmentPassed,
    s.ApprenticeshipAssessmentsPassed,
    s.LastAssessedOn,
    s.HidePassedNotification,
    b.UserId,
    b.Email,
    b.FirstName,
    b.LastName,
    a.UserId,
    a.Email,
    a.FirstName,
    a.LastName
FROM Pttcd.ApprenticeshipQASubmissions s
JOIN Pttcd.Users b ON s.SubmittedByUserId = b.UserId
LEFT JOIN Pttcd.Users a ON s.LastAssessedByUserId = a.UserId
WHERE s.ApprenticeshipQASubmissionId = @LatestApprenticeshipQASubmissionId

SELECT
    s.ApprenticeshipQASubmissionApprenticeshipId,
    s.ApprenticeshipId,
    s.ApprenticeshipTitle,
    s.ApprenticeshipMarketingInformation
FROM Pttcd.ApprenticeshipQASubmissionApprenticeships s
WHERE s.ApprenticeshipQASubmissionId = @LatestApprenticeshipQASubmissionId

SELECT
    l.ApprenticeshipQASubmissionApprenticeshipLocationId,
    l.ApprenticeshipQASubmissionApprenticeshipId,
    l.ApprenticeshipLocationType,
    l.[National],
    l.RegionIds,
    l.VenueName,
    l.DeliveryModes,
    l.Radius
FROM Pttcd.ApprenticeshipQASubmissionApprenticeships s
JOIN Pttcd.ApprenticeshipQASubmissionApprenticeshipLocations l ON s.ApprenticeshipQASubmissionApprenticeshipId = l.ApprenticeshipQASubmissionApprenticeshipId
WHERE s.ApprenticeshipQASubmissionId = @LatestApprenticeshipQASubmissionId";

            var paramz = new { query.ProviderId };

            using (var reader = await transaction.Connection.QueryMultipleAsync(sql, paramz, transaction))
            {
                var submission = reader.Read<Header, UserInfo, UserInfo, ApprenticeshipQASubmission>(
                    (h, submittedBy, assessedBy) => new ApprenticeshipQASubmission()
                    {
                        ApprenticeshipAssessmentsPassed = h.ApprenticeshipAssessmentsPassed,
                        ApprenticeshipQASubmissionId = h.ApprenticeshipQASubmissionId,
                        LastAssessedBy = assessedBy,
                        LastAssessedOn = h.LastAssessedOn,
                        Passed = h.Passed,
                        ProviderAssessmentPassed = h.ProviderAssessmentPassed,
                        ProviderId = h.ProviderId,
                        ProviderMarketingInformation = h.ProviderMarketingInformation,
                        SubmittedByUser = submittedBy,
                        SubmittedOn = h.SubmittedOn,
                        HidePassedNotification = h.HidePassedNotification
                    },
                    splitOn: "UserId,UserId").SingleOrDefault();

                if (submission == null)
                {
                    return new None();
                }

                var apps = reader.Read<ApprenticeshipQASubmissionApprenticeship>();
                submission.Apprenticeships = apps.AsList();

                var locations = reader.Read<Location>();

                var appLocations = apps.ToDictionary(
                    a => a.ApprenticeshipQASubmissionApprenticeshipId,
                    _ => new List<ApprenticeshipQASubmissionApprenticeshipLocation>());

                foreach (var location in locations)
                {
                    if (location.ApprenticeshipLocationType == ApprenticeshipLocationType.ClassroomBased)
                    {
                        appLocations[location.ApprenticeshipQASubmissionApprenticeshipId].Add(
                            new ApprenticeshipQASubmissionApprenticeshipLocation(
                                new ApprenticeshipQASubmissionApprenticeshipClassroomLocation()
                                {
                                    DeliveryModes = location.DeliveryModes
                                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                        .Select(v => Enum.Parse<ApprenticeshipDeliveryMode>(v))
                                        .ToList(),
                                    Radius = location.Radius.Value,
                                    VenueName = location.VenueName
                                }));
                    }
                    else  // location.ApprenticeshipLocationType == ApprenticeshipLocationType.EmployerBased
                    {
                        appLocations[location.ApprenticeshipQASubmissionApprenticeshipId].Add(
                            new ApprenticeshipQASubmissionApprenticeshipLocation(
                                location.National == true ?
                                    new ApprenticeshipQASubmissionApprenticeshipEmployerLocation(new National()) :
                                    new ApprenticeshipQASubmissionApprenticeshipEmployerLocation(
                                        new ApprenticeshipQASubmissionApprenticeshipEmployerLocationRegions()
                                        {
                                            SubRegionIds = location.RegionIds.Split(',').ToList()
                                        })));
                    }
                }

                foreach (var app in apps)
                {
                    app.Locations = appLocations[app.ApprenticeshipQASubmissionApprenticeshipId];
                }

                return submission;
            }
        }

        private class Header
        {
            public int ApprenticeshipQASubmissionId { get; set; }
            public Guid ProviderId { get; set; }
            public DateTime SubmittedOn { get; set; }
            public string ProviderMarketingInformation { get; set; }
            public bool? Passed { get; set; }
            public DateTime? LastAssessedOn { get; set; }
            public bool? ProviderAssessmentPassed { get; set; }
            public bool? ApprenticeshipAssessmentsPassed { get; set; }
            public bool HidePassedNotification { get; set; }
        }

        private class Location
        {
            public int ApprenticeshipQASubmissionApprenticeshipLocationId { get; set; }
            public int ApprenticeshipQASubmissionApprenticeshipId { get; set; }
            public ApprenticeshipLocationType ApprenticeshipLocationType { get; set; }
            public bool? National { get; set; }
            public string RegionIds { get; set; }
            public string VenueName { get; set; }
            public string DeliveryModes { get; set; }
            public int? Radius { get; set; }
        }
    }
}
