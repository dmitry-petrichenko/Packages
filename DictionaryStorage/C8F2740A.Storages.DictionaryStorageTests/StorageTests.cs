using System.IO;
using C8F2740A.Storages.DictionaryStorage;
using Xunit;

namespace C8F2740A.Storages.DictionaryStorageTests
{
    public class StorageTests
    {
        private const string PATH = "store";
        
        private Storage _sut;

        [Fact]
        public void Constructor_WhenNoFile_ShouldCreateEmpty()
        {
            // Arrange
            Clear();
            var before = File.Exists(PATH);

            // Act
            _sut = new Storage(PATH);
            var after = File.Exists(PATH);
            var text = File.ReadAllText(PATH);
            
            // Assert
            Assert.False(before);
            Assert.True(after);
            Assert.Equal("{}", text);
        }
        
        [Fact]
        public void Constructor_WhenFileExist_ShouldRead()
        {
            // Arrange
            Clear();
            File.WriteAllText(PATH, "{\"param1\":\"v1\"}");

            // Act
            _sut = new Storage(PATH);
            var (success, value) = _sut.TryGetValueByKey("param1");
            
            // Assert
            Assert.True(success);
            Assert.Equal("v1", value);
        }
        
        [Fact]
        public void Constructor_WhenEmptyFileExist_ShouldRead()
        {
            // Arrange
            Clear();

            // Act
            File.WriteAllText(PATH, "{}");
            _sut = new Storage(PATH);
            var (success, value) = _sut.TryGetValueByKey("param1");
            
            // Assert
            Assert.False(success);
        }
        
        [Fact]
        public void AddKeyValue_WhenCalledAndCommit_ShouldAddValue()
        {
            // Arrange
            Clear();
            _sut = new Storage(PATH);
            
            // Act
            _sut.AddKeyValue("disco", "value");
            _sut.Commit();

            // Assert
            var text = File.ReadAllText(PATH);
            Assert.Equal("{\"disco\":\"value\"}", text);
        }
        
        [Fact]
        public void AddKeyValue_WhenCalledNoCommit_ShouldNotAddValue()
        {
            // Arrange
            Clear();
            _sut = new Storage(PATH);
            
            // Act
            _sut.AddKeyValue("disco", "value");

            // Assert
            var text = File.ReadAllText(PATH);
            Assert.Equal("{}", text);
        }
        
        [Fact]
        public void AddKeyValue_WhenCalledWithExisting_ShouldReplace()
        {
            // Arrange
            Clear();
            File.WriteAllText(PATH, "{\"par\":\"anyyy\"}");
            _sut = new Storage(PATH);
            
            // Act
            _sut.AddKeyValue("par", "mary");
            _sut.Commit();

            // Assert
            var text = File.ReadAllText(PATH);
            Assert.Equal("{\"par\":\"mary\"}", text);
        }
        
        [Fact]
        public void RemoveKey_WhenCalledWithExisting_ShouldRemove()
        {
            // Arrange
            Clear();
            File.WriteAllText(PATH, "{\"par\":\"anyyy\"}");
            _sut = new Storage(PATH);
            
            // Act
            _sut.RemoveKey("par");
            _sut.Commit();

            // Assert
            var text = File.ReadAllText(PATH);
            Assert.Equal("{}", text);
        }
        
        [Fact]
        public void RemoveKey_WithNotExisting_ShouldDoNothing()
        {
            // Arrange
            Clear();
            File.WriteAllText(PATH, "{\"par\":\"anyyy\"}");
            _sut = new Storage(PATH);
            
            // Act
            _sut.RemoveKey("war");
            _sut.Commit();

            // Assert
            var text = File.ReadAllText(PATH);
            Assert.Equal("{\"par\":\"anyyy\"}", text);
        }
        
        [Fact]
        public void TryGetValueByKey_WithNotExisting_ShouldReturnFalse()
        {
            // Arrange
            Clear();
            File.WriteAllText(PATH, "{\"par\":\"anyyy\"}");
            _sut = new Storage(PATH);
            
            // Act
            var (success, value) = _sut.TryGetValueByKey("war");

            // Assert
            Assert.False(success);
            Assert.Equal(default(string), value);
        }
        
        [Fact]
        public void TryGetValueByKey_WithExistingKey_ShouldReturnValue()
        {
            // Arrange
            Clear();
            File.WriteAllText(PATH, "{\"par\":\"anyyy\"}");
            _sut = new Storage(PATH);
            
            // Act
            var (success, value) = _sut.TryGetValueByKey("par");

            // Assert
            Assert.True(success);
            Assert.Equal("anyyy", value);
        }
        
        private void Clear()
        {
            File.Delete(PATH);
        }
    }
}