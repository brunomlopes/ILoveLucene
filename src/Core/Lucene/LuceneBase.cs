using System.IO;
using System.Reflection;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Lucene.Net.Util;

namespace Core.Lucene
{
    public class LuceneBase
    {
        protected SimpleFSDirectory Directory;

        public LuceneBase()
        {
            var indexDirectory =
                new DirectoryInfo(Path.Combine(new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName,
                                               "index"));
            Directory = new SimpleFSDirectory(indexDirectory);
        }

        protected IndexWriter GetIndexWriter()
        {
            return new IndexWriter(Directory, new StandardAnalyzer(Version.LUCENE_29),
                                   IndexWriter.MaxFieldLength.UNLIMITED);
        }
    }
}