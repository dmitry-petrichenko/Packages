﻿using System.Collections.Generic;
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
        (bool, IEnumerable<string>) GetAll();
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
            var elements = _liteCollection.Find(Query.Contains("value", key));
            var bsonValues = elements as BsonDocument[] ?? elements.ToArray();
            if (!bsonValues.Any())
            {
                return false;
            }
            
            foreach (var element in bsonValues)
            {
                var deleteresult = _liteCollection.Delete(element.GetId());
                if (!deleteresult)
                {
                    return false;
                }
            }
            
            _liteDatabase.Commit();

            return true;
        }

        public (bool, IEnumerable<string>) GetAll()
        {
            string f(string v)
            {
                return v;
            }
            var all = _liteCollection
                .FindAll()
                .Select(e => f(e.GetValue()))
                .ToArray();

            return (true, all);
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