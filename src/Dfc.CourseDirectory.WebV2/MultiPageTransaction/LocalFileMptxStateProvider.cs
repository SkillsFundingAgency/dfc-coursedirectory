#if DEBUG
using System;
using System.Collections.Generic;
using System.IO;
using Dfc.CourseDirectory.WebV2.MultiPageTransaction.Json;
using Newtonsoft.Json;

namespace Dfc.CourseDirectory.WebV2.MultiPageTransaction
{
    public class LocalFileMptxStateProvider : IMptxStateProvider
    {
        private readonly string _dbFilePath;
        private readonly JsonSerializerSettings _serializerSettings;

        public LocalFileMptxStateProvider()
        {
            _dbFilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "CourseDirectory.dev",
                "state.json");

            EnsureDataFolderExists(_dbFilePath);

            _serializerSettings = Settings.CreateSerializerSettings();
        }

        public MptxInstance CreateInstance(
            Type stateType,
            string parentInstanceId,
            object state,
            IReadOnlyDictionary<string, object> items)
        {
            var instanceId = CreateInstanceId();

            items ??= new Dictionary<string, object>();

            var entry = new DbFileEntry()
            {
                StateType = stateType,
                Items = items,
                State = state,
                ParentInstanceId = parentInstanceId,
                ChildInstanceIds = new HashSet<string>()
            };

            object parentState = null;
            Type parentStateType = null;

            WithDbFile(dbFile =>
            {
                if (parentInstanceId != null)
                {
                    if (dbFile.Entries.TryGetValue(parentInstanceId, out var parentEntry))
                    {
                        parentState = parentEntry.State;
                        parentStateType = parentEntry.StateType;

                        parentEntry.ChildInstanceIds.Add(instanceId);
                    }
                    else
                    {
                        throw new InvalidParentException(parentInstanceId);
                    }
                }

                dbFile.Entries.Add(instanceId, entry);
            });

            return new MptxInstance(
                instanceId,
                stateType,
                state,
                parentInstanceId,
                parentStateType,
                parentState,
                items);
        }

        public void DeleteInstance(string instanceId) => WithDbFile(
            dbFile =>
            {
                if (dbFile.Entries.TryGetValue(instanceId, out var entry))
                {
                    if (entry.ChildInstanceIds != null)
                    {
                        foreach (var child in entry.ChildInstanceIds)
                        {
                            dbFile.Entries.Remove(child);
                        }
                    }

                    dbFile.Entries.Remove(instanceId);
                }
            });

        public MptxInstance GetInstance(string instanceId)
        {
            DbFileEntry entry = null;

            object parentState = null;
            Type parentStateType = null;

            WithDbFile(dbFile =>
            {
                dbFile.Entries.TryGetValue(instanceId, out entry);

                if (entry != null && entry.ParentInstanceId != null)
                {
                    var parentEntry = dbFile.Entries[entry.ParentInstanceId];

                    parentState = parentEntry.State;
                    parentStateType = parentEntry.StateType;
                }
            });

            if (entry == null || (entry.ParentInstanceId != null && parentState == null))
            {
                return null;
            }

            return new MptxInstance(
                instanceId,
                entry.StateType,
                entry.State,
                entry.ParentInstanceId,
                parentStateType,
                parentState,
                entry.Items);
        }

        public void SetInstanceState(string instanceId, object state) =>
            WithDbFile(dbFile => dbFile.Entries[instanceId].State = state);

        private static string CreateInstanceId() => Guid.NewGuid().ToString();

        private static void EnsureDataFolderExists(string dbFilePath)
        {
            var folder = Path.GetDirectoryName(dbFilePath);
            
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
        }

        private void WithDbFile(Action<DbFile> updateDb)
        {
            DbFile dbFile = null;

            var serializer = JsonSerializer.Create(_serializerSettings);
            serializer.Error += Serializer_Error;

            try
            {
                if (File.Exists(_dbFilePath))
                {
                    using (var streamReader = File.OpenText(_dbFilePath))
                    using (var jsonReader = new JsonTextReader(streamReader))
                    {
                        dbFile = serializer.Deserialize<DbFile>(jsonReader);
                    }
                }

                // Maybe the outermost object failed to deserialize (or file didn't exist)
                dbFile ??= new DbFile();
                dbFile.Entries ??= new Dictionary<string, DbFileEntry>();

                // If serializing any entries failed they will be null - remove them
                foreach (var e in dbFile.Entries)
                {
                    if (e.Value == null)
                    {
                        dbFile.Entries.Remove(e.Key);
                    }
                }

                updateDb(dbFile);

                using (var streamWriter = File.CreateText(_dbFilePath))
                using (var jsonWriter = new JsonTextWriter(streamWriter))
                {
                    serializer.Serialize(jsonWriter, dbFile);
                }
            }
            finally
            {
                serializer.Error -= Serializer_Error;
            }

            void Serializer_Error(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs e) =>
                e.ErrorContext.Handled = true;
        }

        private class DbFile
        {
            public Dictionary<string, DbFileEntry> Entries { get; set; }
        }

        private class DbFileEntry
        {
            public Type StateType { get; set; }
            public IReadOnlyDictionary<string, object> Items { get; set; }
            public object State { get; set; }
            public string ParentInstanceId { get; set; }
            public HashSet<string> ChildInstanceIds { get; set; }
        }
    }
}
#endif
