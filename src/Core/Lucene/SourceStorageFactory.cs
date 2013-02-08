using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Core.API;
using Core.Abstractions;

namespace Core.Lucene
{
    public class SourceStorageFactory
    {
        private readonly IDirectoryFactory _directoryFactory;
        private readonly IConverterRepository _converterRepository;
        private readonly ILearningRepository _learningRepository;

        [ImportMany]
        public IEnumerable<IItemSource> Sources { get; set; }

        public SourceStorageFactory(IDirectoryFactory directoryFactory, IConverterRepository converterRepository, ILearningRepository learningRepository)
        {
            _directoryFactory = directoryFactory;
            _converterRepository = converterRepository;
            _learningRepository = learningRepository;
        }

        public IEnumerable<SourceStorage> GetAllSourceStorages()
        {
            return Sources.Select(For);
        }

        public SourceStorage SourceStorageFor(string sourceId)
        {
            var source = Sources.SingleOrDefault(s => s.Id == sourceId);
            if (source == null)
            {
                throw new InvalidOperationException(string.Format("No source found for id {0}", sourceId));
            }
            return For(source);
        }

        private SourceStorage For(IItemSource source)
        {
            return new SourceStorage(_directoryFactory.DirectoryFor(source.Id,
                                                                    source.Persistent),
                                     _learningRepository,
                                     _converterRepository);
        }
    }
}