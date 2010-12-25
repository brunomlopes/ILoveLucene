using System.Collections.Generic;
using Lucene.Net.Store;

namespace Core.Lucene
{
    public interface IDirectoryFactory
    {
        Directory DirectoryFor(string id, bool persistent = false);
        IEnumerable<Directory> GetAllDirectories();
    }
}