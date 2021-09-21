using System;
using System.Collections.Generic;
using System.Linq;
using Dfc.CourseDirectory.Core.DataStore.CosmosDb.Models;
using Newtonsoft.Json.Linq;

namespace Dfc.CourseDirectory.Testing.DataStore.CosmosDb
{
    public class InMemoryDocumentStore
    {
        public InMemoryDocumentCollection<ProgType> ProgTypes { get; set; } = new InMemoryDocumentCollection<ProgType>();
        public InMemoryDocumentCollection<Provider> Providers { get; } = new InMemoryDocumentCollection<Provider>();
        public InMemoryDocumentCollection<SectorSubjectAreaTier1> SectorSubjectAreaTier1s { get; set; } = new InMemoryDocumentCollection<SectorSubjectAreaTier1>();
        public InMemoryDocumentCollection<SectorSubjectAreaTier2> SectorSubjectAreaTier2s { get; set; } = new InMemoryDocumentCollection<SectorSubjectAreaTier2>();
        public InMemoryDocumentCollection<Standard> Standards { get; } = new InMemoryDocumentCollection<Standard>();
        public InMemoryDocumentCollection<StandardSectorCode> StandardSectorCodes { get; } = new InMemoryDocumentCollection<StandardSectorCode>();

        public void Clear()
        {
            ProgTypes.Clear();
            Providers.Clear();
            SectorSubjectAreaTier1s.Clear();
            SectorSubjectAreaTier2s.Clear();
            Standards.Clear();
            StandardSectorCodes.Clear();
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

        public void CreateOrUpdate(Func<T, bool> find, Func<T> create, Action<T> update)
        {
            var doc = All.SingleOrDefault(find) ?? create();
            update(doc);
            Save(doc);
        }

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
