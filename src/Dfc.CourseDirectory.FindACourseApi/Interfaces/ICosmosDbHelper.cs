
using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Dfc.CourseDirectory.FindACourseApi.Models;


namespace Dfc.CourseDirectory.FindACourseApi.Interfaces
{
    public interface ICosmosDbHelper
    {
        DocumentClient GetClient();
        Task<Database> CreateDatabaseIfNotExistsAsync(DocumentClient client);
        Task<DocumentCollection> CreateDocumentCollectionIfNotExistsAsync(DocumentClient client, string collectionId);
        Task<Document> CreateDocumentAsync(DocumentClient client, string collectionId, object document);
        T DocumentTo<T>(Document document);
        IEnumerable<T> DocumentsTo<T>(IEnumerable<Document> documents);
        Document GetDocumentById<T>(DocumentClient client, string collectionId, T id);
        Task<Document> UpdateDocumentAsync(DocumentClient client, string collectionId, object document);
        List<Course> GetDocumentsByUKPRN(DocumentClient client, string collectionId, int UKPRN);
        Task<List<string>> DeleteDocumentsByUKPRN(DocumentClient client, string collectionId, int UKPRN);
    }
}
