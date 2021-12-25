using C8F2740A.Automation.InstagramAutomation.Stores;

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
            var fileSystem = new FileSystem(path);
            var storage = new Storage(fileSystem);
            
            return storage;
        }
    }
}