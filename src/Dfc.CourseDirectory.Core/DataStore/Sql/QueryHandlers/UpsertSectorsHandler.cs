using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using OneOf.Types;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class UpsertSectorsHandler : ISqlQueryHandler<UpsertSectors, None>
    {
        public async Task<None> Execute(SqlTransaction transaction, UpsertSectors query)
        {
            var sql = @"
                    INSERT INTO [Pttcd].[Sectors] ([Id], [Code], [Description]) 
                    VALUES
                    (1,		'ENVIRONMENTAL',	'Agriculture, environmental and animal care'),
                    (2,		'BUSINESSADMIN',	'Business and administration'),
                    (3,		'CARE',				'Care services'),
                    (4,		'CATERINGHOSP',		'Catering and hospitality'),
                    (5,		'CONSTRUCTION',		'Construction and the built environment'),
                    (6,		'CREATIVE',			'Creative and design'),
                    (7,		'DIGITAL',			'Digital'),
                    (8,		'EDUCATION',		'Education and early years'),
                    (9,		'ENGMAN',			'Engineering and manufacturing'),
                    (10,	'BEAUTY',			'Hair and beauty'),
                    (11,	'HEALTH',			'Health and science'),
                    (12,	'LEGALFINANCE',		'Legal, finance and accounting'),
                    (13,	'PROTECTIVE',		'Protective services'),
                    (14,	'SALESMARKETING',	'Sales, marketing and procurement'),
                    (15,	'TRANSPORT',		'Transport and logistics')
                    ";

            await transaction.Connection.ExecuteAsync(sql, transaction: transaction);

            return new None();
        }
    }
}
