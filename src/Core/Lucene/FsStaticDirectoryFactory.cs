using System.IO;
using Lucene.Net.Store;

namespace Core.Lucene
{
    public class FsStaticDirectoryFactory : StaticDirectoryFactory
    {
        public FsStaticDirectoryFactory(DirectoryInfo root)
            : base(new SimpleFSDirectory(root))
        {

        }

    }
}