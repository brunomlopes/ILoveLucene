using System.IO;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Lucene.Net.Util;
using System.Linq;

namespace Core.Lucene
{
    public class FsStaticDirectoryFactory : StaticDirectoryFactory
    {
        public FsStaticDirectoryFactory(DirectoryInfo root)
            : base(new SimpleFSDirectory(root))
        {
            if(!IndexExists(root))
            {
                CreateIndex();
            }

        }

        private void CreateIndex()
        {
            new IndexWriter(Directory, new StandardAnalyzer(Version.LUCENE_29), true,
                            IndexWriter.MaxFieldLength.UNLIMITED)
                .Close();
        }

        private bool IndexExists(DirectoryInfo root)
        {
            return (root.Exists && root.EnumerateFiles().Count() > 0);
        }
    }
}