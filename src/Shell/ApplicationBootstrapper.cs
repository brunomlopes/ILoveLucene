using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Autofac;
using Caliburn.Micro;
using Core;
using Core.API;
using Core.Abstractions;
using Core.Lucene;
using Core.Scheduler;
using ElevationHelper.Services;
using ElevationHelper.Services.Infrastructure;
using ILoveLucene.AutoUpdate;
using ILoveLucene.Infrastructure;
using ILoveLucene.Loggers;
using ILoveLucene.Modules;
using ILoveLucene.ViewModels;
using NLog;
using Plugins.IronPython;
using Plugins.Processes;
using Plugins.Tasks;
using Quartz;
using Quartz.Impl;
using Quartz.Simpl;
using ILog = Core.Abstractions.ILog;
using LogManager = NLog.LogManager;
using Task = System.Threading.Tasks.Task;
using Core.Extensions;

namespace ILoveLucene
{
    public class ApplicationBootstrapper : TypedAutofacBootStrapper<MainWindowViewModel>
    {
        private CompositionContainer MefContainer;

        protected override void ConfigureContainer(ContainerBuilder builder)
        {
            base.ConfigureContainer(builder);

            var assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var catalogs = new List<ComposablePartCatalog>
                               {
                                   new AggregateCatalog(AssemblySource.Instance.Select(x => new AssemblyCatalog(x))),
                                   new DirectoryCatalog(assemblyDirectory, "Plugins.*.dll"),
                                   new DirectoryCatalog(assemblyDirectory, "Plugins.dll"),
                                   new AssemblyCatalog(typeof (IItem).Assembly)
                               };

            var pluginsPath = Path.Combine(assemblyDirectory, "Plugins");
            if (Directory.Exists(pluginsPath))
            {
                catalogs.Add(new DirectoryCatalog(pluginsPath, "Plugins.*.dll"));
                catalogs.Add(new DirectoryCatalog(pluginsPath, "Plugins.dll"));
            }
            else
            {
                pluginsPath = assemblyDirectory;
            }

            MefContainer =
                CompositionHost.Initialize(catalogs.ToArray());

            var loadConfiguration =
                new LoadConfiguration(new DirectoryInfo(Path.Combine(assemblyDirectory, "Configuration")), MefContainer);
            var localConfigurationDirectory = new DirectoryInfo(Path.Combine(assemblyDirectory, "Local.Configuration"));
            if(localConfigurationDirectory.Exists)
                loadConfiguration.AddConfigurationLocation(localConfigurationDirectory);
            loadConfiguration.Load();

            var dataDirectory = Path.Combine(assemblyDirectory, "Data");
            var coreConfiguration = new CoreConfiguration(dataDirectory, pluginsPath);

            var learningStorageLocation = new DirectoryInfo(Path.Combine(coreConfiguration.DataDirectory, "Learnings"));
            var indexStorageLocation = new DirectoryInfo(Path.Combine(coreConfiguration.DataDirectory, "Index"));

            var updateManagerAdapter = new UpdateManagerAdapter();
            
            var scheduler = new StdSchedulerFactory().GetScheduler();
            scheduler.JobFactory = new MefJobFactory(new SimpleJobFactory(), MefContainer);

            var batch = new CompositionBatch();
            batch.AddExportedValue(MefContainer);
            batch.AddExportedValue<ILoadConfiguration>(loadConfiguration);
            batch.AddExportedValue<ILog>(new NLogAdapterToCoreILog(NLog.LogManager.GetLogger("mef")));
            batch.AddExportedValue(coreConfiguration);
            batch.AddExportedValue(updateManagerAdapter);
            batch.AddExportedValue<IScheduler>(scheduler);
            MefContainer.Compose(batch);

            MefContainer.SatisfyImportsOnce(updateManagerAdapter);

            builder.RegisterInstance(MefContainer).AsSelf();
            builder.RegisterInstance(coreConfiguration).AsSelf();
            builder.RegisterInstance(updateManagerAdapter).AsSelf();

            builder.RegisterInstance(scheduler).As<IScheduler>();
            builder.RegisterInstance<IWindowManager>(new WindowManager());
            builder.RegisterInstance<IEventAggregator>(new EventAggregator());

            builder.RegisterModule(new LoggingModule(t => new NLogAdapterToCoreILog(NLog.LogManager.GetLogger(t.FullName)),
                                                     t => NLog.LogManager.GetLogger(t.FullName)));
            builder.RegisterModule(new SatisfyMefImports(MefContainer));

            builder.RegisterType<MainWindowViewModel>().AsSelf();
            builder.RegisterType<AutoCompleteBasedOnLucene>().AsSelf();
            builder.RegisterType<GetActionsForItem>().As<IGetActionsForItem>();

            builder.RegisterType<ConverterRepository>().As<IConverterRepository>();

            builder.RegisterType<SourceStorageFactory>().AsSelf().SingleInstance();

            builder.RegisterType<FileSystemLearningRepository>().As<ILearningRepository>().WithParameter("input", learningStorageLocation);
            builder.RegisterType<ScheduleIndexJobs>().As<IStartupTask>();
            builder.RegisterType<ScheduleUpdateCheckJob>().As<IStartupTask>();
            builder.RegisterType<IronPythonCommandsMefExport>().As<IStartupTask>();
            builder.RegisterType<LogScheduledJobs>().As<IStartupTask>();
            builder.RegisterType<ProcessMonitor>().As<IStartupTask>();

            builder.RegisterType<Shutdown>().AsSelf();

            builder.RegisterType<SeparateIndexesDirectoryFactory>()
                .As<IDirectoryFactory>().WithParameter("root", indexStorageLocation)
                .SingleInstance();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            var hack = new CompositionBatch();
            hack.AddExportedValue(Container);
            MefContainer.Compose(hack);

            var startupTasksLogger = LogManager.GetLogger("StartupTasks");
            Task.Factory.StartNew(() => ExecuteStartupTasks(startupTasksLogger))
                .GuardForException(ex => startupTasksLogger.ErrorException("Error with startup tasks", ex))
                ;

            base.OnStartup(sender, e);
        }

        private void ExecuteStartupTasks(Logger log)
        {
            var startupTasks = Container.Resolve<IEnumerable<IStartupTask>>();

            // HACK: only start the scheduler after resolving startup tasks
            // because one of the startup tasks has an onimportstatified that runs execute (ScheduleIndexJobs)
            // which we want to run just once
            Container.Resolve<IScheduler>().Start();

            foreach (var t in startupTasks)
            {
                try
                {
                    log.Info("Startup task {0} starting", t.GetType().FullName);
                    t.Execute();
                    log.Info("Startup task {0} done", t.GetType().FullName);
                }
                catch (Exception e)
                {
                    log.ErrorException("Error executing task "+t.GetType().Name, e);
                }
            }
        }

        protected override void OnExit(object sender, EventArgs e)
        {
            Container.Resolve<Shutdown>().Now();

            base.OnExit(sender, e);
        }
    }
}