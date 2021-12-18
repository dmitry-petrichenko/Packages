namespace C8F2740A.Storages.DictionaryStorage
{
    public interface IStorageApi
    {
        void AddKeyValue(string key, string value);
        (bool, string) TryGetValueByKey(string key);
        void RemoveKey(string key);
    }
}