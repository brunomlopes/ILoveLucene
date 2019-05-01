using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Directory = Lucene.Net.Store.Directory;

namespace Core.Lucene
{
    public class SeparateIndexesDirectoryFactory : IDirectoryFactory
    {
        private readonly DirectoryInfo _root;
        private readonly Dictionary<string, RAMDirectory> _inMemoryDirectories;

        public SeparateIndexesDirectoryFactory(DirectoryInfo root)
        {
            _root = root;
            _inMemoryDirectories = new Dictionary<string, RAMDirectory>();
        }

        public Directory DirectoryFor(string id, bool persistent)
        {
            if (persistent)
            {
                var info = new DirectoryInfo(Path.Combine(_root.FullName, id+".index"));

                var directory = new MMapDirectory(info);
                if (!info.Exists || !info.EnumerateFiles().Any())
                {
                    var config = new IndexWriterConfig(LuceneVersion.LUCENE_48, new StandardAnalyzer(LuceneVersion.LUCENE_48));
                    config.OpenMode = OpenMode.CREATE_OR_APPEND;
                    new IndexWriter(directory, config)
                        .Dispose();
                }
                
                return directory;
            }
            else
            {
                if (!_inMemoryDirectories.ContainsKey(id))
                {
                    _inMemoryDirectories[id] = new RAMDirectory();
                }
                return _inMemoryDirectories[id];
            }
        }

        public IEnumerable<Directory> GetAllDirectories()
        {
            _root.Refresh();
            return _root.EnumerateDirectories("*.index")
                .Select(d => new MMapDirectory(d))
                .Cast<Directory>()
                .Concat(_inMemoryDirectories.Values);
        }
    }
}