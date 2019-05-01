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
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Util;

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

            var directoryInfo = new DirectoryInfo(indexDirectory);
            if(directoryInfo.Exists)
            {
                directoryInfo.Delete(true);
                directoryInfo.Refresh();
            }

            var mergedDirectory = new SimpleFSDirectory(directoryInfo);
            var conf = new IndexWriterConfig(LuceneVersion.LUCENE_48, new SimpleAnalyzer(LuceneVersion.LUCENE_48));
            var mergedIndex = new IndexWriter(mergedDirectory, conf);

            var directoryFactory = AutofacContainer.Resolve<IDirectoryFactory>();
            var allDirectories = directoryFactory.GetAllDirectories();
            foreach(var directory in allDirectories)
            {
                using(var reader = DirectoryReader.Open(directory))
                {
                    mergedIndex.AddIndexes(reader);
                }
            }
            mergedIndex.Commit();

        }
    }
}