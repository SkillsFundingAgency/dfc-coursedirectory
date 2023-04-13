using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Dfc.CourseDirectory.Core.DataStore.CosmosDb.StoredProcedures
{
    public class StoredProcedureDeployHelper
    {
        private readonly DocumentClient _documentClient;
        private readonly Configuration _configuration;
        private Task _deploymentTask;

        public StoredProcedureDeployHelper(DocumentClient documentClient, Configuration configuration)
        {
            _documentClient = documentClient;
            _configuration = configuration;
        }

        public Task EnsureStoredProceduresAreDeployed() =>
            LazyInitializer.EnsureInitialized(ref _deploymentTask, DeployStoredProcedures);

        private async Task DeployStoredProcedures()
        {
            await DeployStoredProcedureToCollection(_configuration.CoursesCollectionName, "ArchiveCoursesForProvider");
            async Task DeployStoredProcedureToCollection(string collection, string storedProcedureName)
            {
                var scriptFilePath = $"{typeof(StoredProcedureDeployHelper).Namespace}.{storedProcedureName}.js";

                using var stream = typeof(StoredProcedureDeployHelper).Assembly.GetManifestResourceStream(scriptFilePath);
                if (stream == null)
                {
                    throw new ArgumentException(
                        $"Cannot find stored procedure '{scriptFilePath}'.",
                        nameof(storedProcedureName));
                }

                using var reader = new StreamReader(stream);
                var script = reader.ReadToEnd();

                var storedProcId = storedProcedureName;
                var collectionUri = UriFactory.CreateDocumentCollectionUri(_configuration.DatabaseId, collection);

                var storedProcedure = new StoredProcedure()
                {
                    Body = script,
                    Id = storedProcId
                };

                try
                {
                    await _documentClient.CreateStoredProcedureAsync(collectionUri, storedProcedure);
                }
                catch (DocumentClientException dex) when (dex.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    // Already exists - replace it

                    var sprocUri = UriFactory.CreateStoredProcedureUri(_configuration.DatabaseId, collection, storedProcId);
                    await _documentClient.ReplaceStoredProcedureAsync(sprocUri, storedProcedure);
                }
            }
        }
    }
}
