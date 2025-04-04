﻿using Dapper;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Microsoft.Azure.Functions.Worker;

namespace Dfc.CourseDirectory.Functions
{
    public class AddMissingProviderIdForeignKeys
    {
        private readonly ISqlQueryDispatcherFactory _sqlQueryDispatcherFactory;

        public AddMissingProviderIdForeignKeys(ISqlQueryDispatcherFactory sqlQueryDispatcherFactory)
        {
            _sqlQueryDispatcherFactory = sqlQueryDispatcherFactory;
        }

        [Function(nameof(AddMissingProviderIdForeignKeys))]
        public async Task Execute([HttpTrigger(AuthorizationLevel.Function, "get", "post")] string input)
        {
            await ExecuteBatchedStatement(@"
UPDATE Pttcd.Courses
SET ProviderId = p.ProviderId
FROM Pttcd.Courses c
JOIN Pttcd.Providers p ON c.ProviderUkprn = p.Ukprn
WHERE c.ProviderId IS NULL");

            await ExecuteBatchedStatement(@"
UPDATE Pttcd.Venues
SET ProviderId = p.ProviderId
FROM Pttcd.Venues v
JOIN Pttcd.Providers p ON v.ProviderUkprn = p.Ukprn
WHERE v.ProviderId IS NULL");

            async Task ExecuteBatchedStatement(string sqlStatement)
            {
                const int batchSize = 200;

                int updated;

                do
                {
                    using var dispatcher = _sqlQueryDispatcherFactory.CreateDispatcher();

                    var batchedStatement = $@"
SET ROWCOUNT {batchSize}

{sqlStatement}

SELECT @@ROWCOUNT";

                    updated = await dispatcher.Transaction.Connection.QuerySingleAsync<int>(batchedStatement, transaction: dispatcher.Transaction);

                    await dispatcher.Commit();
                }
                while (updated == batchSize);
            }
        }
    }
}
