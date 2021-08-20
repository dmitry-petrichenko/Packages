namespace C8F2740A.Storage.QueuesStorage
{
    public interface IStorageFactory
    {
        IStorage Create(string settingsPath);
    }
}