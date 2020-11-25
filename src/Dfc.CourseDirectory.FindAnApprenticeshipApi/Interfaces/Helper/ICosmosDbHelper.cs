using Dfc.Providerportal.FindAnApprenticeship.Models;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dfc.Providerportal.FindAnApprenticeship.Interfaces.Helper
{
    public interface ICosmosDbHelper
    {
        DocumentClient GetClient();
        DocumentClient GetTcpClient();

        Task<Database> CreateDatabaseIfNotExistsAsync(DocumentClient client);
        Task<DocumentCollection> CreateDocumentCollectionIfNotExistsAsync(DocumentClient client, string collectionId);
        Task<Document> CreateDocumentAsync(DocumentClient client, string collectionId, object document);
        T DocumentTo<T>(Document document);
        IEnumerable<T> DocumentsTo<T>(IEnumerable<Document> documents);
        List<Apprenticeship> GetApprenticeshipByUKPRN(DocumentClient client, string collectionId, int UKPRN);
        Document GetDocumentById<T>(DocumentClient client, string collectionId, T id);
        Task<Document> UpdateDocumentAsync(DocumentClient client, string collectionId, object document);
        List<StandardsAndFrameworks> GetStandardsAndFrameworksBySearch(DocumentClient client, string collectionId, string search);
        List<StandardsAndFrameworks> GetProgTypesForFramework(DocumentClient client, string collectionId, List<StandardsAndFrameworks> docs);
        Task<List<string>> DeleteBulkUploadApprenticeships(DocumentClient client, string collectionId, int UKPRN);
        Task<List<string>> DeleteDocumentsByUKPRN(DocumentClient client, string collectionId, int UKPRN);
        List<Apprenticeship> GetApprenticeshipCollection(DocumentClient client, string collectionId);
        List<Apprenticeship> GetLiveApprenticeships(DocumentClient client, string collectionId);
    }
}
