using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Core.Abstractions;

namespace Core.Lucene
{
    public class SourceStorageFactory
    {
        private readonly LuceneStorage _luceneStorage;
        private readonly IDirectoryFactory _directoryFactory;

        [ImportMany]
        public IEnumerable<IItemSource> Sources { get; set; }

        public SourceStorageFactory(LuceneStorage luceneStorage, IDirectoryFactory directoryFactory)
        {
            _luceneStorage = luceneStorage;
            _directoryFactory = directoryFactory;
        }

        public IEnumerable<SourceStorage> GetAllSourceStorages()
        {
            return Sources.Select(source => new SourceStorage(source,
                                                              _directoryFactory.DirectoryFor(source.Id,
                                                                                             source.Persistent),
                                                              _luceneStorage));
        }

        public SourceStorage SourceStorageFor(string sourceId)
        {
            var source = Sources.SingleOrDefault(s => s.Id == sourceId);
            if (source == null)
            {
                throw new InvalidOperationException(string.Format("No source found for id {0}", sourceId));
            }
            return new SourceStorage(source, _directoryFactory.DirectoryFor(source.Id, source.Persistent), _luceneStorage);
        }
    }
}