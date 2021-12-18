using System.Collections.Generic;
using System.IO;
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
        
        private IStorageDesigner _dictionaryStorageDesigner;
        
        public Storage(string path)
        {
            _path = path;

            if (!File.Exists(_path))
            {
                InitializeEmptyDictionaryStorage();
                return;
            }
            
            var rawText = File.ReadAllText(_path);
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
            File.WriteAllText(_path, rawText);
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
            File.WriteAllText(_path, rawText);
        }
    }
}