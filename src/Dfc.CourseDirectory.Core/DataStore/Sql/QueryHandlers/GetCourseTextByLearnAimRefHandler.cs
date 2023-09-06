using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetCourseTextByLearnAimRefHandler : ISqlQueryHandler<GetCourseTextByLearnAimRef, CourseText>
    {
        public async Task<CourseText> Execute(SqlTransaction transaction, GetCourseTextByLearnAimRef query)
        {


            var sql = @$"
SELECT  LearnAimRef,
        QualificationCourseTitle,
        NotionalNVQLevelv2,
        TypeName,
        AwardOrgCode,
        CourseDescription,
        EntryRequirements,
        WhatYoullLearn,
        HowYoullLearn,
        WhatYoullNeed,
        HowYoullBeAssessed,
        WhereNext
FROM    LARS.CourseText
WHERE   Lars = @{nameof(query.LearnAimRef)}";

            return await transaction.Connection.QuerySingleOrDefaultAsync<CourseText>(sql, query, transaction);
        }
    }
}
