﻿using System;
using System.Threading.Tasks;
using Dfc.CourseDirectory.WebV2.DataStore.Sql.Queries;
using Dfc.CourseDirectory.WebV2.Models;

namespace Dfc.CourseDirectory.WebV2.Tests
{
    public partial class TestData
    {
        public Task CreateApprenticeshipQAApprenticeshipAssessment(
            int apprenticeshipQASubmissionId,
            Guid apprenticeshipId,
            string assessedByUserId,
            DateTime assessedOn,
            bool compliancePassed,
            string complianceComments,
            ApprenticeshipQAApprenticeshipComplianceFailedReasons complianceFailedReasons,
            bool stylePassed,
            string styleComments,
            ApprenticeshipQAApprenticeshipStyleFailedReasons styleFailedReasons
        ) => WithSqlQueryDispatcher(dispatcher => dispatcher.ExecuteQuery(
            new CreateApprenticeshipQAApprenticeshipAssessment()
            {
                ApprenticeshipQASubmissionId = apprenticeshipQASubmissionId,
                ApprenticeshipId = apprenticeshipId,
                AssessedByUserId = assessedByUserId,
                AssessedOn = assessedOn,
                CompliancePassed = compliancePassed,
                ComplianceComments = complianceComments,
                ComplianceFailedReasons = complianceFailedReasons,
                Passed = compliancePassed && stylePassed,
                StylePassed = stylePassed,
                StyleComments = styleComments,
                StyleFailedReasons = styleFailedReasons
            }));
    }
}
