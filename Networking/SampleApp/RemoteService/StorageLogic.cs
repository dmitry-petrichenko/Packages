using C8F2740A.Storage.QueuesStorage;

namespace SampleService
{
    public class StorageLogic
    {
        private readonly IStorage _storage;
        
        public StorageLogic(IStorage storage)
        {
            _storage = storage;
        }

        public void AddValue(string value)
        {
            var q = _storage.GetQueue("test");
            q.Enqueue(value);
        }

        public string GetCurrent()
        {
            var q = _storage.GetQueue("test");
            return q.GetCurrent().Item2;
        }
        
        public string PopValue()
        {
            var q = _storage.GetQueue("test");
            return q.Dequeue().Item2;
        }
    }
}