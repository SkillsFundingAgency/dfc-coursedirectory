﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Queries;
using Dfc.CourseDirectory.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dfc.CourseDirectory.Functions
{
    public class DeleteDuplicateProviderRecords
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<DeleteDuplicateProviderRecords> _logger;
        private readonly ICosmosDbQueryDispatcher _cosmosDbQueryDispatcher;
        private readonly DocumentClient _documentClient;

        public DeleteDuplicateProviderRecords(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<DeleteDuplicateProviderRecords> logger,
            ICosmosDbQueryDispatcher cosmosDbQueryDispatcher,
            DocumentClient documentClient)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
            _cosmosDbQueryDispatcher = cosmosDbQueryDispatcher;
            _documentClient = documentClient;
        }

        [FunctionName(nameof(DeleteDuplicateProviderRecords))]
        [Singleton]
        public Task<IActionResult> Run(
            [HttpTrigger(authLevel: AuthorizationLevel.Anonymous, "POST")] HttpRequest httpRequest)
        {
            return FindAndRemoveDuplicateProviderRecords(dryRun: false);
        }

        [FunctionName("GetDuplicateProviderRecords")]
        [Singleton]
        public Task<IActionResult> DryRun(
            [HttpTrigger(authLevel: AuthorizationLevel.Anonymous, "GET")] HttpRequest httpRequest)
        {
            return FindAndRemoveDuplicateProviderRecords(dryRun: true);
        }

        private async Task<IActionResult> FindAndRemoveDuplicateProviderRecords(bool dryRun)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            using var sqlConnection = scope.ServiceProvider.GetRequiredService<SqlConnection>();

            var sql = @"
select
	Ukprn,
	0 AS ProviderType  -- This value gets fixed up next
into #DuplicateProviders
from pttcd.Providers
group by Ukprn
having count(*) > 1


-- Pre-flight check: ensure there are no apprenticeships linked to providers that have duplicated records
if exists (
    select 1 from pttcd.apprenticeships a
    join pttcd.providers p on a.ProviderId = p.ProviderId
    join #DuplicateProviders x on p.Ukprn = x.Ukprn)
begin
    raiserror('Found apprenticeship records linked to a provider that has duplicate records', 16, 1)
    return
end

-- Pre-flight check: ensure the DisplayNameSource is the default for providers that have duplicated records
if exists (
    select 1 from pttcd.providers p
    join #DuplicateProviders x on p.Ukprn = x.Ukprn
    where p.DisplayNameSource <> 0)
begin
    raiserror('Found provider with duplicate records that has a non-default DisplayNameSource', 16, 1)
    return
end


-- Consolidate the ProviderType for all records for a given UKPRN, ensuring we don't remove ProviderType(s)

update #DuplicateProviders
set ProviderType = ProviderType | 1
from #DuplicateProviders x
join (select Ukprn from Pttcd.Providers where ProviderType & 1 <> 0 group by Ukprn) p on p.Ukprn = x.Ukprn

update #DuplicateProviders
set ProviderType = ProviderType | 2
from #DuplicateProviders x
join (select Ukprn from Pttcd.Providers where ProviderType & 2 <> 0 group by Ukprn) p on p.Ukprn = x.Ukprn


-- Pick the provider records to remove (keeping the most recently updated for a given UKPRN)
;
with DuplicateProviders as (
	select ProviderId, Ukprn, ROW_NUMBER() over (partition by Ukprn order by Ukprn, UpdatedOn desc) RowNum
	from Pttcd.Providers
	where Ukprn in (
		select Ukprn from #DuplicateProviders
	)
)
select ProviderId, Ukprn into #DeletingProviderIds from DuplicateProviders
where RowNum > 1


-- Find the remaining provider IDs where their ProviderType has been changed as a result of the consolidation above
select p.ProviderId, p.Ukprn, p.ProviderType OldProviderType, y.ProviderType NewProviderType
from Pttcd.Providers p
left join #DeletingProviderIds x on x.ProviderId = p.ProviderId
join #DuplicateProviders y on p.Ukprn = y.Ukprn
where x.ProviderId is null and y.ProviderType <> p.ProviderType


if @DryRun = 0
begin
	-- Remove provider records
	delete from Pttcd.ProviderContacts where ProviderId in (select ProviderId from #DeletingProviderIds)
	delete from Pttcd.Providers where ProviderId in (select ProviderId from #DeletingProviderIds)

	-- Assign consolidate ProviderType
	update Pttcd.Providers
	set ProviderType = x.ProviderType
	from Pttcd.Providers p
	join #DuplicateProviders x on p.Ukprn = x.Ukprn
end


-- Return the provider records we've deleted
select d.ProviderId, p.Ukprn from #DeletingProviderIds d
join #DuplicateProviders p ON d.Ukprn = p.Ukprn


drop table #DuplicateProviders
drop table #DeletingProviderIds";

            IList<UpdatedProviderInfo> updatedProviders;
            IList<DeletedProviderInfo> deletedProviders;

            using (var reader = await sqlConnection.QueryMultipleAsync(sql, new { DryRun = dryRun }))
            {
                updatedProviders = (await reader.ReadAsync<UpdatedProviderInfo>()).AsList();
                deletedProviders = (await reader.ReadAsync<DeletedProviderInfo>()).AsList();
            }

            if (!dryRun)
            {
                foreach (var record in updatedProviders)
                {
                    await _cosmosDbQueryDispatcher.ExecuteQuery(new UpdateProviderType()
                    {
                        ProviderId = record.ProviderId,
                        ProviderType = record.NewProviderType
                    });
                }

                foreach (var record in deletedProviders)
                {
                    var documentLink = UriFactory.CreateDocumentUri(
                        databaseId: "providerportal",
                        collectionId: "ukrlp",
                        documentId: record.ProviderId.ToString());

                    await _documentClient.DeleteDocumentAsync(documentLink);
                }
            }

            return new OkObjectResult(new
            {
                updatedProviders,
                deletedProviders
            });
        }

        private class UpdatedProviderInfo
        {
            public Guid ProviderId { get; set; }
            public int Ukprn { get; set; }
            public ProviderType NewProviderType { get; set; }
            public ProviderType OldProviderType { get; set; }
        }

        private class DeletedProviderInfo
        {
            public Guid ProviderId { get; set; }
            public int Ukprn { get; set; }
        }
    }
}
