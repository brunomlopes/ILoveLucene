using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Reflection;
using Core.Abstractions;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Directory = Lucene.Net.Store.Directory;
using Version = Lucene.Net.Util.Version;
using System.Linq;

namespace Core.Lucene
{
    public class LuceneBase  
    {
        public Directory Directory { get; private set; }
        protected DirectoryInfo LearningStorageLocation;
        protected LuceneStorage Storage;

        private IEnumerable<IConverter> _converters;

        [ImportMany]
        public IEnumerable<IConverter> Converters
        {
            get { return _converters; }
            set
            {
                _converters = value;
                Storage.SetConverters(Converters);
            }
        }

        public LuceneBase()
        {
            var root = new FileInfo(Assembly.GetCallingAssembly().Location).DirectoryName;
            var indexDirectory =
                new DirectoryInfo(Path.Combine(root,
                                               "index"));
            Directory = new SimpleFSDirectory(indexDirectory);
            LearningStorageLocation = new DirectoryInfo(Path.Combine(root, "learnings"));
            Storage = new LuceneStorage(new IConverter[]{}, LearningStorageLocation);

        }

        protected LuceneBase(Directory directory, DirectoryInfo learningStorageLocation)
        {
            Directory = directory;
            LearningStorageLocation = learningStorageLocation;
            Storage = new LuceneStorage(new IConverter[] { }, LearningStorageLocation);

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