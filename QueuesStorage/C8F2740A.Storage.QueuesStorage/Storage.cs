using LiteDB;
using Microsoft.Extensions.Configuration;

namespace C8F2740A.Storage.QueuesStorage
{
    public interface IStorage
    {
        IQueue GetQueue(string name);
        void Dispose();
    }
    
    public class Storage : IStorage
    {
        private readonly ILiteDatabase _liteDatabase;
        
        public Storage(IConfiguration configuration)
        {
            _liteDatabase = new LiteDatabase(configuration["DATABASE_PATH"]);
        }

        public IQueue GetQueue(string name)
        {
            var collection = _liteDatabase.GetCollection<BsonDocument>(name);
            var queue = new Queue(collection);

            return queue;
        }

        public void Dispose()
        {
            _liteDatabase.Dispose();
        }
    }
}