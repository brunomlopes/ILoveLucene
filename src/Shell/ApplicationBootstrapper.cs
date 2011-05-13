using System;
using System.Collections.Generic;
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
using ElevationHelper.Services;
using ILoveLucene.Infrastructure;
using ILoveLucene.Loggers;
using ILoveLucene.Modules;
using ILoveLucene.ViewModels;
using Quartz;
using Quartz.Impl;
using Quartz.Simpl;
using ILog = Core.Abstractions.ILog;

namespace ILoveLucene
{
    public class ApplicationBootstrapper : TypedAutofacBootStrapper<MainWindowViewModel>
    {
        private CompositionContainer MefContainer;

        protected override void ConfigureContainer(ContainerBuilder builder)
        {
            base.ConfigureContainer(builder);

            var assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var catalogs = new ComposablePartCatalog[]
                               {
                                   new AggregateCatalog(
                                       AssemblySource.Instance.Select(x => new AssemblyCatalog(x))
                                           .OfType<ComposablePartCatalog>()),
                                   new DirectoryCatalog(assemblyDirectory, "Plugins.*.dll"),
                                   new DirectoryCatalog(assemblyDirectory, "Plugins.dll"),
                                   new AssemblyCatalog(typeof (IItem).Assembly)
                               };

            MefContainer =
                CompositionHost.Initialize(catalogs);

            var loadConfiguration =
                new LoadConfiguration(new DirectoryInfo(Path.Combine(assemblyDirectory, "Configuration")));
            loadConfiguration.Load(MefContainer);

            var root = new FileInfo(Assembly.GetCallingAssembly().Location).DirectoryName;
            var learningStorageLocation = new DirectoryInfo(Path.Combine(root, "learnings"));
            var indexStorageLocation = new DirectoryInfo(Path.Combine(root, "index"));
            var logFileLocation = new FileInfo(Path.Combine(root, "log.txt"));

            var scheduler = new StdSchedulerFactory().GetScheduler();
            scheduler.JobFactory = new MefJobFactory(new SimpleJobFactory(), MefContainer);

            var batch = new CompositionBatch();
            batch.AddExportedValue(MefContainer);
            batch.AddExportedValue<ILoadConfiguration>(loadConfiguration);
            batch.AddExportedValue<ILog>(new FileLogger(logFileLocation, tag: "mef"));
            MefContainer.Compose(batch);

            builder.RegisterInstance(MefContainer).AsSelf();

            builder.RegisterInstance(scheduler).As<IScheduler>();
            builder.RegisterInstance<IWindowManager>(new WindowManager());
            builder.RegisterInstance<IEventAggregator>(new EventAggregator());

            builder.RegisterModule(new LoggingModule(t => new FileLogger(logFileLocation, t.Name)));
            builder.RegisterModule(new SatisfyMefImports(MefContainer));

            builder.RegisterType<MainWindowViewModel>().AsSelf();
            builder.RegisterType<AutoCompleteBasedOnLucene>().As<IAutoCompleteText>();
            builder.RegisterType<GetActionsForItem>().As<IGetActionsForItem>();

            builder.RegisterType<ConverterRepository>().As<IConverterRepository>();

            builder.RegisterType<LuceneStorage>().AsSelf().SingleInstance();
            builder.RegisterType<SourceStorageFactory>().AsSelf().SingleInstance();

            builder.RegisterType<FileSystemLearningRepository>().As<ILearningRepository>().WithParameter("input", learningStorageLocation);
            builder.RegisterType<ScheduleIndexJobs>().As<IStartupTask>();

            builder.RegisterType<SeparateIndexesDirectoryFactory>()
                .As<IDirectoryFactory>().WithParameter("root", indexStorageLocation)
                .SingleInstance();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            base.OnStartup(sender, e);
            
            Container.Resolve<IScheduler>().Start();
            Container.Resolve<IEnumerable<IStartupTask>>().AsParallel().ForAll(t => t.Execute());
        }

        protected override void OnExit(object sender, EventArgs e)
        {
            Container.Resolve<IScheduler>().Shutdown();
            var elevatedChannel = new ElevatedChannel<IStopTheElevationHelper>();
            if (elevatedChannel.ElevationProcessExists())
                elevatedChannel.GetElevatedHandler().Stop();

            base.OnExit(sender, e);
        }
    }
}