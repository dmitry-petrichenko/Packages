using System.Collections.Generic;
using System.IO;
using C8F2740A.Storages.QueuesStorage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace C8F2740A.Storages.QueuesStorageTests
{
    public class StorageTests
    {
        private readonly string _path;
        
        public StorageTests()
        {
            _path = "storage.db";
        }

        [Fact]
        public void GetQueue_WhenCalled_ShouldReturnSameQueue()
        {
            var configuration = new ConfigurationMock();
            configuration.AddKey("DATABASE_PATH", _path);
            var storage = new Storage(configuration);
            var queue1 = GetTestQueue("queue1", storage);
            var queue2 = GetTestQueue("queue2", storage);
            var queue3 = GetTestQueue("queue1", storage);

            Assert.False(IsEqualQueues(queue1, queue2));
            Assert.True(IsEqualQueues(queue1, queue3));

            ClearTest(storage);
        }

        private bool IsEqualQueues(IQueue queue1, IQueue queue2)
        {
            var queue1Value = queue1.GetCurrent().Item2;
            var queue2Value = queue2.GetCurrent().Item2;

            return queue1Value.Equals(queue2Value);
        }

        private IQueue GetTestQueue(string name, Storage storage)
        {
            var queue = storage.GetQueue(name);
            queue.Enqueue($"{name}Value");
            return queue;
        }

        private void ClearTest(Storage storage)
        {
            storage.Dispose();
            File.Delete(_path);
        }
    }

    public class ConfigurationMock : IConfiguration
    {
        private Dictionary<string, string> _dictionary;
        
        public ConfigurationMock()
        {
            _dictionary = new Dictionary<string, string>();
            _dictionary["df"] = "";
        }

        public void AddKey(string key, string value)
        {
            _dictionary.Add(key, value);
        }

        public IConfigurationSection GetSection(string key)
        {
            return null;
        }

        public IEnumerable<IConfigurationSection> GetChildren()
        {
            return null;
        }

        public IChangeToken GetReloadToken()
        {
            return null;
        }

        public string this[string key]
        {
            get => _dictionary[key];
            set => _dictionary[key] = value;
        }
    }
}