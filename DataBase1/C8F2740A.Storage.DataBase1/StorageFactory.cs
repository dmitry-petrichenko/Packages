using Microsoft.Extensions.Configuration;

namespace C8F2740A.Storage.DataBase1
{
    public interface IStorageFactory
    {
        IStorage Create(string settingsPath);
    }
    
    public class StorageFactory : IStorageFactory
    {
        public IStorage Create(string settingsPath)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(settingsPath)
                .Build();
            
            return new Storage(configuration);
        }
    }
}