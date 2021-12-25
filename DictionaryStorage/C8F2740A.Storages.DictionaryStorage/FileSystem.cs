using System.IO;

namespace C8F2740A.Automation.InstagramAutomation.Stores
{
    public interface IFileSystem
    {
        string ReadAllText();
        void WriteAllText(string contents);
        bool Exists();
    }
    
    public class FileSystem : IFileSystem
    {
        private readonly string _path;
        
        public FileSystem(string path)
        {
            _path = path;
        }

        public string ReadAllText()
        {
            return File.ReadAllText(_path);
        }

        public void WriteAllText(string contents)
        {
            File.WriteAllText(_path, contents);
        }

        public bool Exists()
        {
            return File.Exists(_path);
        }
    }
}