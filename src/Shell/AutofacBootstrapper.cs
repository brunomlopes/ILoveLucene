using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Integration.Mef;
using Caliburn.Micro;
using Core.Abstractions;
using Core.Lucene;
using ILoveLucene.Commands;
using ILoveLucene.Modules;
using ILoveLucene.ViewModels;

namespace ILoveLucene
{
    public class AutofacBootstrapper : TypedAutofacBootStrapper<IShell>
    {
        protected override void ConfigureContainer(ContainerBuilder builder)
        {
            base.ConfigureContainer(builder);

            var assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            //var container = new CompositionContainer();
            builder.RegisterComposablePartCatalog(
                new AggregateCatalog(
                    AssemblySource.Instance.Select(x => new AssemblyCatalog(x)).OfType<ComposablePartCatalog>()));
            builder.RegisterComposablePartCatalog(new DirectoryCatalog(assemblyDirectory));

            builder.RegisterInstance<IWindowManager>(new WindowManager());
            builder.RegisterInstance<IEventAggregator>(new EventAggregator());
            builder.RegisterType<MainWindowViewModel>().As<IShell>();

            builder.RegisterModule(new LoggingModule());

            builder.RegisterType<AutoCompleteBasedOnLucene>().As<IAutoCompleteText>();

            builder.Register(c => c.Resolve(typeof (IEnumerable<ICommand>)))
                .As<Fake>()
                .Exported(c => c.As<IEnumerable<ICommand>>());
            //builder.RegisterAssemblyTypes(typeof(AutoCompleteBasedOnLucene).Assembly)
            //    .Where(t => t.IsPublic && t.Namespace != typeof(Core.Abstractions.ICommand).Namespace)
            //    .AsImplementedInterfaces()
            //    .SingleInstance();

            //builder.RegisterAssemblyTypes(this.GetType().Assembly)
            //    .InNamespaceOf<ExitApplication>()
            //    .AsImplementedInterfaces()
            //    .SingleInstance();

            //var batch = new CompositionBatch();

            //batch.AddExportedValue<IWindowManager>(new WindowManager());
            //batch.AddExportedValue<IEventAggregator>(new EventAggregator());
            //batch.AddExportedValue(Container);
        }

        public interface Fake
        {
            
        }
    }
}