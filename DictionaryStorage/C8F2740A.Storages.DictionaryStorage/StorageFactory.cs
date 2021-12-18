namespace C8F2740A.Storages.DictionaryStorage
{
    public interface IStorageFactory
    {
        IStorage Create(string path);
    }
    
    public class StorageFactory : IStorageFactory
    {
        public IStorage Create(string path)
        {
            var storage = new Storage(path);
            
            return storage;
        }
    }
}