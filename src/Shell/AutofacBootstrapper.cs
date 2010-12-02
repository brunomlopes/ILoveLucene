using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Autofac;
using Caliburn.Micro;
using Core;
using Core.Abstractions;
using Core.Lucene;
using Core.Scheduler;
using ILoveLucene.Modules;
using ILoveLucene.ViewModels;
using Quartz;
using Quartz.Impl;
using Quartz.Simpl;

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

            var scheduler = new StdSchedulerFactory().GetScheduler();
            scheduler.JobFactory = new MefJobFactory(new SimpleJobFactory(), MefContainer);

            var batch = new CompositionBatch();
            batch.AddExportedValue(MefContainer);
            batch.AddExportedValue<ILoadConfiguration>(loadConfiguration);
            batch.AddExportedValue(scheduler);
            MefContainer.Compose(batch);

            builder.RegisterInstance(MefContainer).AsSelf();

            builder.RegisterInstance<IWindowManager>(new WindowManager());
            builder.RegisterInstance<IEventAggregator>(new EventAggregator());

            builder.RegisterModule(new LoggingModule());

            builder.RegisterType<MainWindowViewModel>().As<IShell>();
            builder.RegisterType<AutoCompleteBasedOnLucene>().As<IAutoCompleteText>();
            builder.RegisterType<GetActionsForItem>().As<IGetActionsForItem>();

        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            base.OnStartup(sender, e);
            MefContainer.GetExportedValues<IStartupTask>().AsParallel().ForAll(t => t.Execute());
            MefContainer.GetExportedValue<IScheduler>().StartDelayed(TimeSpan.FromMinutes(1));
        }

        protected override void OnExit(object sender, EventArgs e)
        {
            MefContainer.GetExportedValue<IScheduler>().Shutdown();
            base.OnExit(sender, e);
        }
    }
}