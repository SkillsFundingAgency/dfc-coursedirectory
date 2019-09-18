using System;
using System.Collections.Generic;
using System.Text;

namespace Dfc.CourseDirectory.Services.Tests.Unit.Helpers
{
    public class SampleJsons
    {
        #region Successful Json Files

        public static string SuccessfulStandardFile()
        {
            return
                "[{\r\n  \"id\": \"00000000-0000-0000-0000-000000000000\",\r\n  \"standardCode\": 157,\r\n  \"version\": 1,\r\n  \"standardName\": \"Standard Name\",\r\n  \"standardSectorCode\": \"42\",\r\n  \"urlLink\": \"https://www.gov.uk/government/\",\r\n  \"notionalEndLevel\": \"4\",\r\n  \"otherBodyApprovalRequired\": null,\r\n  \"apprenticeshipType\": 0,\r\n  \"effectiveFrom\": \"2019-08-01T00:00:00\",\r\n  \"createdDateTimeUtc\": \"2019-08-22T14:32:53.983\",\r\n  \"modifiedDateTimeUtc\": \"2019-03-08T21:01:29.993\",\r\n  \"recordStatusId\": 1,\r\n  \"alreadyCreated\": false,\r\n  \"frameworkCode\": null,\r\n  \"progType\": null,\r\n  \"pathwayCode\": null,\r\n  \"pathwayName\": null,\r\n  \"nasTitle\": null,\r\n  \"effectiveTo\": \"0001-01-01T00:00:00\",\r\n  \"sectorSubjectAreaTier1\": null,\r\n  \"sectorSubjectAreaTier2\": null,\r\n  \"progTypeDesc\": null,\r\n  \"progTypeDesc2\": null\r\n}]";
        }
        public static string SuccessfulFrameworkFile()
        {
            return
                "[{\r\n  \"id\": \"00000000-0000-0000-0000-000000000000\",\r\n  \"standardCode\": null,\r\n  \"version\": null,\r\n  \"standardName\": \"Standard Name\",\r\n  \"standardSectorCode\": \"42\",\r\n  \"urlLink\": \"https://www.gov.uk/government/\",\r\n  \"notionalEndLevel\": \"4\",\r\n  \"otherBodyApprovalRequired\": null,\r\n  \"apprenticeshipType\": 0,\r\n  \"effectiveFrom\": \"2019-08-01T00:00:00\",\r\n  \"createdDateTimeUtc\": \"2019-08-22T14:32:53.983\",\r\n  \"modifiedDateTimeUtc\": \"2019-03-08T21:01:29.993\",\r\n  \"recordStatusId\": 1,\r\n  \"alreadyCreated\": false,\r\n  \"frameworkCode\": 3,\r\n  \"progType\": 4,\r\n  \"pathwayCode\": 5,\r\n  \"pathwayName\": null,\r\n  \"nasTitle\": null,\r\n  \"effectiveTo\": \"0001-01-01T00:00:00\",\r\n  \"sectorSubjectAreaTier1\": null,\r\n  \"sectorSubjectAreaTier2\": null,\r\n  \"progTypeDesc\": null,\r\n  \"progTypeDesc2\": null\r\n}]";
        }
        #endregion

        #region Unsuccessful files

        public static string EmptyFile()
        {
            return "[]";
        }
        

        #endregion
    }
}
