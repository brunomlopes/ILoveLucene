using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Reflection;
using Core.Abstractions;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Directory = Lucene.Net.Store.Directory;
using Version = Lucene.Net.Util.Version;

namespace Core.Lucene
{
    public class LuceneBase  
    {
        protected readonly IDirectoryFactory DirectoryFactory;
        public Directory Directory { get; private set; }
        protected DirectoryInfo LearningStorageLocation;
        protected LuceneStorage Storage;

        [ImportMany]
        public IEnumerable<IConverter> Converters { get; set; }

        public LuceneBase(IDirectoryFactory directoryFactory, LuceneStorage storage)
        {
            DirectoryFactory = directoryFactory;
            Storage = storage;

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

    public interface IDirectoryFactory
    {
        Directory DirectoryFor(string name, bool persistent = false);
        IEnumerable<Directory> GetAllDirectories();
    }

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

        public Directory DirectoryFor(string name, bool persistent = false)
        {
            return _directory;
        }

        public IEnumerable<Directory> GetAllDirectories()
        {
            return new[] {_directory};
        }
    }

    public class FsStaticDirectoryFactory : StaticDirectoryFactory
    {
        public FsStaticDirectoryFactory(DirectoryInfo root)
            : base(new SimpleFSDirectory(root))
        {
        }

    }
}