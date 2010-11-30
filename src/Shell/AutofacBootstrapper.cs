using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using Core;
using Core.Abstractions;
using Core.Lucene;
using Core.Scheduler;
using ILoveLucene.Modules;
using ILoveLucene.ViewModels;
using Core.Extensions;

namespace ILoveLucene
{
    public class AutofacBootstrapper : TypedAutofacBootStrapper<IShell>
    {
        private CompositionContainer MefContainer;

        protected override void ConfigureContainer(ContainerBuilder builder)
        {
            base.ConfigureContainer(builder);

            var assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);


            MefContainer =
                CompositionHost.Initialize(new AggregateCatalog(
                                               AssemblySource.Instance.Select(x => new AssemblyCatalog(x))
                                                   .OfType<ComposablePartCatalog>()),
                                           new DirectoryCatalog(assemblyDirectory, "Plugins.*.dll"),
                                           new DirectoryCatalog(assemblyDirectory, "Plugins.dll"),
                                           new AssemblyCatalog(typeof (Core.Abstractions.IItem).Assembly)
                    );

            var loadConfiguration =
                new LoadConfiguration(new DirectoryInfo(Path.Combine(assemblyDirectory, "Configuration")));
            loadConfiguration
                .Load(MefContainer);

            var batch = new CompositionBatch();
            batch.AddExportedValue(MefContainer);
            batch.AddExportedValue<ILoadConfiguration>(loadConfiguration);
            MefContainer.Compose(batch);

            builder.RegisterInstance(MefContainer).AsSelf();

            builder.RegisterInstance<IWindowManager>(new WindowManager());
            builder.RegisterInstance<IEventAggregator>(new EventAggregator());

            builder.RegisterModule(new LoggingModule());

            builder.RegisterType<MainWindowViewModel>().As<IShell>();
            builder.RegisterType<AutoCompleteBasedOnLucene>().As<IAutoCompleteText>();
            builder.RegisterType<GetActionsForItem>().As<IGetActionsForItem>();

            builder.RegisterType<Indexer>().As<IIndexer>();
            builder.RegisterType<TaskExecuter>().AsSelf();

            builder.RegisterType<Scheduler>().AsSelf();
        }

        protected override void Configure()
        {
            base.Configure();
            Task.Factory
                .StartNew(() => Container.Resolve<Scheduler>().Start())
                .GuardForException(e => Debug.WriteLine("Error running tasks:"+e));
        }

        protected override void OnExit(object sender, EventArgs e)
        {
            Container.Resolve<Scheduler>().Shutdown();
        }
    }
}