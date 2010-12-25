using System.Collections.Generic;
using Lucene.Net.Store;

namespace Core.Lucene
{
    /// <summary>
    /// returns always the same directory
    /// </summary>
    public class StaticDirectoryFactory : IDirectoryFactory
    {
        protected readonly Directory Directory;

        public StaticDirectoryFactory(Directory directory)
        {
            Directory = directory;
        }

        public Directory DirectoryFor(string id, bool persistent = false)
        {
            return Directory;
        }

        public IEnumerable<Directory> GetAllDirectories()
        {
            return new[] {Directory};
        }
    }
}