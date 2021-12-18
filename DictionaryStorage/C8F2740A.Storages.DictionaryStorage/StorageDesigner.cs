using System.Collections.Generic;

namespace C8F2740A.Storages.DictionaryStorage
{
    public interface IStorageDesigner : IStorageApi
    {
        IDictionary<string, string> GetContent();
    }
    
    public class StorageDesigner : IStorageDesigner
    {
        private readonly IDictionary<string, string> _content;
        
        public StorageDesigner(IDictionary<string, string> content)
        {
            _content = content;
        }

        public void AddKeyValue(string key, string value)
        {
            _content[key] = value;
        }

        public (bool, string) TryGetValueByKey(string key)
        {
            if (_content.ContainsKey(key))
            {
                return (true, _content[key]);
            }
            
            return (false, default);
        }

        public void RemoveKey(string key)
        {
            _content.Remove(key);
        }

        public IDictionary<string, string> GetContent()
        {
            return _content;
        }
    }
}