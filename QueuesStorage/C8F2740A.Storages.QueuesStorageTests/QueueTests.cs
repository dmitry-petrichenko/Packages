using System;
using System.IO;
using C8F2740A.Storages.QueuesStorage;
using Xunit;

namespace C8F2740A.Storages.QueuesStorageTests
{
    public class QueueTests
    {
        private readonly string _path;
        
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
            var storage = new Storage(configuration);
            var queue = storage.GetQueue("test1");
            queue.Enqueue(expectedValue);
            
            // Asset
            var current = queue.GetCurrent().Item2;
            
            // Assert
            Assert.Equal(expectedValue, current);

            ClearTest(storage);
        }
        
        [Fact]
        public void GetCurrent_WhenCalledForNotExisting_ShouldReturnFalseAndEmpty()
        {
            // Arrange
            var expectedValue = "test1";
            var configuration = new ConfigurationMock();
            configuration.AddKey("DATABASE_PATH", _path);
            var storage = new Storage(configuration);
            var queue = storage.GetQueue(expectedValue);

            // Asset
            var (result, value) = queue.GetCurrent();

            // Assert
            Assert.False(result);
            Assert.Equal(String.Empty, value);

            ClearTest(storage);
        }
        
        [Fact]
        public void Dequeue_WhenCalledForNotExisting_ShouldReturnFalseAndEmpty()
        {
            // Arrange
            var expectedValue = "test1";
            var configuration = new ConfigurationMock();
            configuration.AddKey("DATABASE_PATH", _path);
            var storage = new Storage(configuration);
            var queue = storage.GetQueue(expectedValue);

            // Asset
            var (result, value) = queue.Dequeue();

            // Assert
            Assert.False(result);
            Assert.Equal(String.Empty, value);

            ClearTest(storage);
        }
        
        [Fact]
        public void Dequeue_WhenCalled_ShouldReturnCorrectly()
        {
            // Arrange
            var expectedValue = "testValue";
            var configuration = new ConfigurationMock();
            configuration.AddKey("DATABASE_PATH", _path);
            var storage = new Storage(configuration);
            var queue = storage.GetQueue(expectedValue);
            queue.Enqueue(expectedValue);

            // Asset
            var (result, value) = queue.Dequeue();

            // Assert
            Assert.True(result);
            Assert.Equal(expectedValue, value);

            ClearTest(storage);
        }
        
        [Fact]
        public void Enqueue_WhenCalled_ShouldEnqueueValue()
        {
            // Arrange
            var expectedValue = "test1";
            var configuration = new ConfigurationMock();
            configuration.AddKey("DATABASE_PATH", _path);
            var storage = new Storage(configuration);
            var queue = storage.GetQueue("someQueue");

            // Asset
            queue.Enqueue(expectedValue);

            // Assert
            var (result, actual) = queue.GetCurrent();
            Assert.True(result);
            Assert.Equal(expectedValue, actual);

            ClearTest(storage);
        }
        
        private void ClearTest(Storage storage)
        {
            storage.Dispose();
            File.Delete(_path);
        }
    }
}