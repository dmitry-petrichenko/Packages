using System;
using System.IO;
using System.Linq;
using C8F2740A.Storages.QueuesStorage;
using Xunit;

namespace C8F2740A.Storages.QueuesStorageTests
{
    public class QueueTests : IDisposable
    {
        private readonly string _path;
        private IStorage _storage;
        
        public QueueTests()
        {
            _path = "storage.db";
        }

        [Fact]
        public void GetCurrent_WhenCalled_ShouldReturnCurrent()
        {
            // Arrange
            var expectedValue = "value1";
            var configuration = new ConfigurationMock();
            configuration.AddKey("DATABASE_PATH", _path);
            _storage = new Storage(configuration);
            var queue = _storage.GetQueue("test1");
            queue.Enqueue(expectedValue);
            
            // Asset
            var current = queue.GetCurrent().Item2;
            
            // Assert
            Assert.Equal(expectedValue, current);
        }
        
        [Fact]
        public void GetCurrent_WhenCalledForNotExisting_ShouldReturnFalseAndEmpty()
        {
            // Arrange
            var expectedValue = "test1";
            var configuration = new ConfigurationMock();
            configuration.AddKey("DATABASE_PATH", _path);
            _storage = new Storage(configuration);
            var queue = _storage.GetQueue(expectedValue);

            // Asset
            var (result, value) = queue.GetCurrent();

            // Assert
            Assert.False(result);
            Assert.Equal(String.Empty, value);
        }
        
        [Fact]
        public void Dequeue_WhenCalledForNotExisting_ShouldReturnFalseAndEmpty()
        {
            // Arrange
            var expectedValue = "test1";
            var configuration = new ConfigurationMock();
            configuration.AddKey("DATABASE_PATH", _path);
            _storage = new Storage(configuration);
            var queue = _storage.GetQueue(expectedValue);

            // Asset
            var (result, value) = queue.Dequeue();

            // Assert
            Assert.False(result);
            Assert.Equal(String.Empty, value);
        }
        
        [Fact]
        public void Dequeue_WhenCalled_ShouldReturnCorrectly()
        {
            // Arrange
            var expectedValue = "testValue";
            var configuration = new ConfigurationMock();
            configuration.AddKey("DATABASE_PATH", _path);
            _storage = new Storage(configuration);
            var queue = _storage.GetQueue(expectedValue);
            queue.Enqueue(expectedValue);

            // Asset
            var (result, value) = queue.Dequeue();

            // Assert
            Assert.True(result);
            Assert.Equal(expectedValue, value);
        }
        
        [Fact]
        public void Enqueue_WhenCalled_ShouldEnqueueValue()
        {
            // Arrange
            var expectedValue = "test1";
            var configuration = new ConfigurationMock();
            configuration.AddKey("DATABASE_PATH", _path);
            _storage = new Storage(configuration);
            var queue = _storage.GetQueue("someQueue");

            // Asset
            queue.Enqueue(expectedValue);

            // Assert
            var (result, actual) = queue.GetCurrent();
            Assert.True(result);
            Assert.Equal(expectedValue, actual);
        }
        
        [Fact]
        public void GetAll_WhenCalled_ShouldReturnAll()
        {
            // Arrange
            var expectedArr = new[] {"1", "12", "18", "20" };
            var configuration = new ConfigurationMock();
            configuration.AddKey("DATABASE_PATH", _path);
            _storage = new Storage(configuration);
            var queue = _storage.GetQueue("someQueue");

            // Asset
            queue.Enqueue("1");
            queue.Enqueue("12");
            queue.Enqueue("18");
            queue.Enqueue("20");

            // Assert
            var (result, actual) = queue.GetAll();
            var actualArr = actual.ToArray();
            Assert.Equal(expectedArr, actualArr);
        }
        
        private void ClearTest(IStorage storage)
        {
            storage.Dispose();
            File.Delete(_path);
        }

        public void Dispose()
        {
            ClearTest(_storage);
        }
    }
}