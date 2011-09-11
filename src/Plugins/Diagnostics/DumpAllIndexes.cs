using System.ComponentModel.Composition;
using System.IO;
using Core;
using Core.API;
using Core.Abstractions;
using Core.Lucene;
using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Plugins.Commands;
using System.Linq;
using Autofac;

namespace Plugins.Diagnostics
{
    [Export(typeof(ICommand))]
    public class DumpAllIndexes : BaseCommand
    {
        [ImportConfiguration]
        public CoreConfiguration CoreConfiguration { get; set; }

        [Import]
        public IContainer AutofacContainer { get; set; }

        public override void Act()
        {
            var indexDirectory = Path.Combine(CoreConfiguration.DataDirectory, "MergedIndexes");
            var mergedDirectory = new SimpleFSDirectory(new DirectoryInfo(indexDirectory));
            var mergedIndex = new IndexWriter(mergedDirectory, new SimpleAnalyzer(), true,
                                              IndexWriter.MaxFieldLength.UNLIMITED);

            var directoryFactory = AutofacContainer.Resolve<IDirectoryFactory>();
            mergedIndex.AddIndexes(directoryFactory.GetAllDirectories().Select(d => IndexReader.Open(d, true)).ToArray());
            mergedIndex.Commit();
        }
    }
}