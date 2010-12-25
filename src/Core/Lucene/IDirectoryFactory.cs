using System;
using System.Collections.Generic;
using Directory = Lucene.Net.Store.Directory;

namespace Core.Lucene
{
    public interface IDirectoryFactory
    {
        Directory DirectoryFor(string id, bool persistent = false);
        IEnumerable<Directory> GetAllDirectories();
    }
}