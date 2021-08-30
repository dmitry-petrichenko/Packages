namespace C8F2740A.Storages.QueuesStorage
{
    public interface IStorageFactory
    {
        IStorage Create(string settingsPath);
    }
}