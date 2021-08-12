using LiteDB;
using Microsoft.Extensions.Configuration;

namespace C8F2740A.Storage.DataBase1
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
    
    public interface IQueue
    {
        (bool, string) GetCurrent();
        (bool, string) Dequeue();
        void Enqueue(string value);
    }

    public class Queue : IQueue
    {
        private readonly ILiteCollection<BsonDocument> _liteCollection;
        
        public Queue(ILiteCollection<BsonDocument> liteCollection)
        {
            _liteCollection = liteCollection;
        }

        private (bool, BsonDocument) GetCurrentInternal(ILiteCollection<BsonDocument> liteCollectio)
        {
            var element = liteCollectio.FindOne(p => true);
            if (element == default)
            {
                return (false, default);
            }
            
            return (true, element); 
        }

        public (bool, string) GetCurrent()
        {
            var (result, element) = GetCurrentInternal(_liteCollection);
            if (!result)
            {
                return (false, string.Empty);
            }
            
            return (true, element.GetValue());
        }

        public (bool, string) Dequeue()
        {
            var (result, element) = GetCurrentInternal(_liteCollection);
            if (!result)
            {
                return (false, string.Empty);
            }

            var s = element.GetValue();
            _liteCollection.Delete(element.GetId());
            
            return (true, s);
        }

        public void Enqueue(string value)
        {
            _liteCollection.Insert(new BsonDocument().SetValue(value));
        }
    }
}