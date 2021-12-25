using System.Collections.Generic;
using C8F2740A.Automation.InstagramAutomation.Stores;
using Newtonsoft.Json;

namespace C8F2740A.Storages.DictionaryStorage
{
    public interface IStorage : IStorageApi
    {
        void Commit();
    }

    public class Storage : IStorage
    {
        private readonly string _path;
        private readonly IFileSystem _fileSystem;
        
        private IStorageDesigner _dictionaryStorageDesigner;
        
        public Storage(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;

            if (!_fileSystem.Exists())
            {
                InitializeEmptyDictionaryStorage();
                return;
            }
            
            var rawText = _fileSystem.ReadAllText();
            var rawDictionary = JsonConvert.DeserializeObject<IDictionary<string, string>>(rawText);

            if (rawDictionary == null)
            {
                InitializeEmptyDictionaryStorage();
                return;
            }
            
            _dictionaryStorageDesigner = new StorageDesigner(rawDictionary);
        }

        private void InitializeEmptyDictionaryStorage()
        {
            var emptyDictionary = new Dictionary<string, string>();
            WriteDictionaryToFile(emptyDictionary);
            _dictionaryStorageDesigner = new StorageDesigner(emptyDictionary);
        }

        private void WriteDictionaryToFile(IDictionary<string, string> dictionary)
        {
            string rawText = JsonConvert.SerializeObject(dictionary);
            _fileSystem.WriteAllText(rawText);
        }

        public void AddKeyValue(string key, string value)
        {
            _dictionaryStorageDesigner.AddKeyValue(key, value);
        }

        public (bool, string) TryGetValueByKey(string key)
        {
            return _dictionaryStorageDesigner.TryGetValueByKey(key);
        }

        public void RemoveKey(string key)
        {
            _dictionaryStorageDesigner.RemoveKey(key);
        }

        public void Commit()
        {
            var rawDictionary = _dictionaryStorageDesigner.GetContent();
            string rawText = JsonConvert.SerializeObject(rawDictionary);
            _fileSystem.WriteAllText(rawText);
        }
    }
}