using System.Data.SqlClient;
using System.Threading.Tasks;
using Dfc.CourseDirectory.Core.DataStore.Sql.Models;
using Dfc.CourseDirectory.Core.DataStore.Sql.Queries;
using Dfc.CourseDirectory.Core.DataStore.Sql;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using OneOf.Types;
using Dapper;

namespace Dfc.CourseDirectory.Core.DataStore.Sql.QueryHandlers
{
    public class ProcessAllProvidersHandler : ISqlQueryHandler<ProcessAllProviders, None>
    {
        public async Task<None> Execute(SqlTransaction transaction, ProcessAllProviders query)
        {


            //var query = client.CreateDocumentQuery<Provider>(
            //    collectionUri,
            //    new FeedOptions() { EnableCrossPartitionQuery = true, MaxItemCount = -1 }).AsDocumentQuery();

            await transaction.Connection.QueryAsync(query.ProcessChunk);

            return new None();
        }
    }
}
