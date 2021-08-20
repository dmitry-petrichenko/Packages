using LiteDB;

namespace C8F2740A.Storage.QueuesStorage
{
    internal static class BsonDocumentExtensions
    {
        public static BsonValue GetId(this BsonDocument bsonDocument)
        {
            return bsonDocument["_id"];
        }
        
        public static BsonValue GetValue(this BsonDocument bsonDocument)
        {
            return bsonDocument["value"];
        }
        
        public static BsonDocument SetValue(this BsonDocument bsonDocument, string value)
        {
            bsonDocument["value"] = value;
            
            return bsonDocument;
        }
    }
}