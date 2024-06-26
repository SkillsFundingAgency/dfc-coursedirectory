﻿using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class GetSectorsAttachedWithCoursesHandler : ISqlQueryHandler<GetSectorsAttachedWithCourses, IReadOnlyCollection<Sector>>
    {
        public async Task<IReadOnlyCollection<Sector>> Execute(SqlTransaction transaction, GetSectorsAttachedWithCourses query)
        {
            var sql = @"SELECT Id, Code, [Description] 
                        FROM Pttcd.Sectors 
                        WHERE Id IN (
                        SELECT DISTINCT SectorId FROM Pttcd.Courses Where SectorId IS NOT NULL AND CourseStatus = 1
                        )";

            var sectors = (await transaction.Connection.QueryAsync<Sector>(sql, transaction: transaction)).AsList();

            return sectors;
        }        
    }
}
