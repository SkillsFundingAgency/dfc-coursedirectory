using System;
using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Newtonsoft.Json.Linq;

namespace Dfc.CourseDirectory.Testing.DataStore.CosmosDb
{
    public class InMemoryDocumentStore
    {
        public InMemoryDocumentCollection<Apprenticeship> Apprenticeships { get; } = new InMemoryDocumentCollection<Apprenticeship>();
        public InMemoryDocumentCollection<Framework> Frameworks { get; } = new InMemoryDocumentCollection<Framework>();
        public InMemoryDocumentCollection<Provider> Providers { get; } = new InMemoryDocumentCollection<Provider>();
        public InMemoryDocumentCollection<Standard> Standards { get; } = new InMemoryDocumentCollection<Standard>();
        public InMemoryDocumentCollection<Venue> Venues { get; } = new InMemoryDocumentCollection<Venue>();

        public void Clear()
        {
            Apprenticeships.Clear();
            Frameworks.Clear();
            Providers.Clear();
            Standards.Clear();
            Venues.Clear();
        }
    }

    public class InMemoryDocumentCollection<T>
    {
        private readonly Dictionary<string, T> _documents;
        private readonly Func<T, string> _idGetter;

        public InMemoryDocumentCollection()
        {
            _documents = new Dictionary<string, T>();
            _idGetter = CreateIdGetterFunction();
        }

        public T this[string id] => CloneDocument(_documents[id]);

        public IReadOnlyCollection<T> All => _documents.Values.Select(CloneDocument).ToList();

        public void Clear() => _documents.Clear();

        public void Delete(string id)
        {
            if (!_documents.ContainsKey(id))
            {
                throw new ArgumentNullException("Document with specified ID does not exist.", nameof(id));
            }

            _documents.Remove(id);
        }

        public void Save(T document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            string id = _idGetter(document);
            _documents[id] = CloneDocument(document);
        }

        private static T CloneDocument(T document) => JObject.FromObject(document).ToObject<T>();

        private static Func<T, string> CreateIdGetterFunction() => doc => JObject.FromObject(doc)["id"].ToString();
    }
}
