using Microsoft.Extensions.Configuration;

namespace C8F2740A.Storages.QueuesStorage
{
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