using System.Linq;
using LiteDB;

namespace C8F2740A.Storages.QueuesStorage
{
    public interface IQueue
    {
        (bool, string) GetCurrent();
        (bool, string) Dequeue();
        void Enqueue(string value);
        bool TryRemoveByValue(string key);
    }
    
    public class Queue : IQueue
    {
        private readonly ILiteCollection<BsonDocument> _liteCollection;
        private readonly ILiteDatabase _liteDatabase;
        
        public Queue(ILiteCollection<BsonDocument> liteCollection, ILiteDatabase liteDatabase)
        {
            _liteCollection = liteCollection;
            _liteDatabase = liteDatabase;
        }

        public bool TryRemoveByValue(string key)
        {
            var elements = _liteCollection.Find(bson => bson.GetValue() == key);

            var bsonValues = elements as BsonDocument[] ?? elements.ToArray();
            if (!bsonValues.Any())
            {
                return false;
            }
            
            foreach (var element in bsonValues)
            {
                _liteCollection.Delete(element);
            }
            
            _liteDatabase.Commit();

            return true;
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
            _liteDatabase.Commit();

            return (true, s);
        }

        public void Enqueue(string value)
        {
            _liteCollection.Insert(new BsonDocument().SetValue(value));
            _liteDatabase.Commit();
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
    }
}