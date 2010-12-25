using System.Collections.Generic;
using Lucene.Net.Store;

namespace Core.Lucene
{
    /// <summary>
    /// returns always the same directory
    /// </summary>
    public class StaticDirectoryFactory : IDirectoryFactory
    {
        private readonly Directory _directory;

        public StaticDirectoryFactory(Directory directory)
        {
            _directory = directory;
        }

        public Directory DirectoryFor(string id, bool persistent = false)
        {
            return _directory;
        }

        public IEnumerable<Directory> GetAllDirectories()
        {
            return new[] {_directory};
        }
    }
}