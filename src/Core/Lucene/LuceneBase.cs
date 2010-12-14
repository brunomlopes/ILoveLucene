using System;
using System.IO;
using System.Reflection;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Directory = Lucene.Net.Store.Directory;
using Version = Lucene.Net.Util.Version;

namespace Core.Lucene
{
    public class LuceneBase
    {
        public Directory Directory { get; private set; }

        public LuceneBase()
        {
            var indexDirectory =
                new DirectoryInfo(Path.Combine(new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName,
                                               "index"));
            Directory = new SimpleFSDirectory(indexDirectory);
        }

        protected LuceneBase(Directory directory)
        {
            Directory = directory;
        }

        public IndexWriter GetIndexWriter()
        {
            return new IndexWriter(Directory, new StandardAnalyzer(Version.LUCENE_29),
                                   IndexWriter.MaxFieldLength.UNLIMITED);
        }
        
        public IndexReader GetIndexReader()
        {
            return IndexReader.Open(Directory, false);
        }
        
        public IndexReader GetReadOnlyIndexReader()
        {
            return IndexReader.Open(Directory, true);
        }
    }
}