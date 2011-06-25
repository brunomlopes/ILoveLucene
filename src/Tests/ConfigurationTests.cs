using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using System.Linq;
using Caliburn.Micro;
using Core;
using Core.Abstractions;
using Newtonsoft.Json;
using Plugins.Shortcuts;
using Xunit;

namespace Tests
{
    public class ConfigurationTests
    {
        private const string _PathToDebugConfiguration = @"..\..\..\Shell\bin\Debug\Configuration";

        [Fact]
        public void CanLoadShortcutConfiguration()
        {
            var container = new CompositionContainer();
            var configurationCatalog = new LoadConfiguration(new DirectoryInfo(_PathToDebugConfiguration), container);
            configurationCatalog.Load();

            var conf = container.GetExportedValue<Configuration>();
            Assert.Contains(@"%USERPROFILE%\AppData\Roaming\Microsoft\Windows\Start Menu", conf.Directories);
        }
        
        [Fact]
        public void ReloadingConfigurationShouldNotDuplicateEntries()
        {
            var container = new CompositionContainer();
            var configurationCatalog = new LoadConfiguration(new DirectoryInfo(_PathToDebugConfiguration), container);
            configurationCatalog.Load();

            var conf = container.GetExportedValue<Configuration>();
            var count = conf.Directories.Count;
            configurationCatalog.Load();
            Assert.Equal(count, conf.Directories.Count);
            Assert.Contains(@"%USERPROFILE%\AppData\Roaming\Microsoft\Windows\Start Menu", conf.Directories);
        }
        
        [Fact]
        public void RequestingNotFoundConfigurationShouldLoadANewlyCreatedConfiguration()
        {
            var container = new CompositionContainer(new AssemblyCatalog(this.GetType().Assembly));

            var configurationDirectory = new DirectoryInfo(@"temp-configuration");
            if (configurationDirectory.Exists)
            {
                configurationDirectory.Delete(true);
            }
            configurationDirectory.Create();

            var configurationCatalog = new LoadConfiguration(configurationDirectory, container);
            configurationCatalog.Load();

            var conf = container.GetExportedValue<SampleConfigurationWithDefaultValues>();
            Assert.Equal(5, conf.Value);
        }
        
        [Fact]
        public void RequestingStoredConfigurationWithADefaultTypeAlreadyRegisteredShouldReturnStoredConfiguration()
        {
            var configurationDirectory = new DirectoryInfo(@"temp-configuration");
        
            var conf = new SampleConfigurationWithDefaultValues() {Value = 10};
            WriteConfiguration<SampleConfigurationWithDefaultValues>(configurationDirectory, conf);

            var container = new CompositionContainer(new AssemblyCatalog(this.GetType().Assembly));
            var configurationCatalog = new LoadConfiguration(configurationDirectory, container);
            configurationCatalog.Load();

            conf = container.GetExportedValue<SampleConfigurationWithDefaultValues>();
            Assert.Equal(10, conf.Value);
        }

        [Fact]
        public void CanLoadConfigurationFromGlobalAndUserRepository()
        {
            var userConfigurationDirectory = new DirectoryInfo("user-configuration");
            var systemConfigurationDirectory = new DirectoryInfo("temp-configuration");

            var systemConfiguration = new SampleConfigurationWithDefaultValues() {Value = 10, SecondValue = 200};
            var userConfiguration = new {Value = 20};
            WriteConfiguration<SampleConfigurationWithDefaultValues>(userConfigurationDirectory, userConfiguration);
            WriteConfiguration<SampleConfigurationWithDefaultValues>(systemConfigurationDirectory, systemConfiguration);

            var container = new CompositionContainer();
            var configurationCatalog = new LoadConfiguration(systemConfigurationDirectory, container);
            configurationCatalog.AddConfigurationLocation(userConfigurationDirectory);

            configurationCatalog.Load();
            var conf = container.GetExportedValue<SampleConfigurationWithDefaultValues>();
            Assert.Equal(20, conf.Value);
            Assert.Equal(200, conf.SecondValue);
        }

        [Fact()]
        public void ComposingTwiceIsNotABigDeal()
        {
            var container = new CompositionContainer();
            var configurationCatalog = new LoadConfiguration(new DirectoryInfo(_PathToDebugConfiguration), container);
            configurationCatalog.Load();

            var conf = container.GetExportedValue<Configuration>();
            Assert.Contains(@"%USERPROFILE%\AppData\Roaming\Microsoft\Windows\Start Menu", conf.Directories);

            configurationCatalog.Load();
            conf = container.GetExportedValue<Configuration>();
            Assert.Contains(@"%USERPROFILE%\AppData\Roaming\Microsoft\Windows\Start Menu", conf.Directories);
        }
        
        [Fact()]
        public void SecondLoadTriggersPartsImportSatisfiedNotification()
        {
            var container = CompositionHost.Initialize(new TypeCatalog(typeof(ClassWithConfigurationAndNotification)));

            var configurationDirectory = new DirectoryInfo(@"temp-configuration");

            var conf = new SampleConfigurationWithDefaultValues() { Value = 10 };
            WriteConfiguration<SampleConfigurationWithDefaultValues>(configurationDirectory, conf);

            var configurationCatalog = new LoadConfiguration(configurationDirectory, container);
            configurationCatalog.Load();

            var instance = container.GetExportedValue<ClassWithConfigurationAndNotification>("A");

            Assert.Equal(1, instance.OnImportsSatisfiedCalled);

            configurationCatalog.Load();
            Assert.Equal(2, instance.OnImportsSatisfiedCalled);
        }

        private void WriteConfiguration<T>(DirectoryInfo configurationDirectory, object conf)
        {
            if (configurationDirectory.Exists)
            {
                configurationDirectory.Delete(true);
            }
            configurationDirectory.Create();

            File.WriteAllText(
                Path.Combine(configurationDirectory.FullName,
                             typeof(T).AssemblyQualifiedName),
                JsonConvert.SerializeObject(conf));
        }
    }

    [Export("A", typeof(ClassWithConfigurationAndNotification))]
    class ClassWithConfigurationAndNotification: IPartImportsSatisfiedNotification
    {
        public int OnImportsSatisfiedCalled;

        [ImportConfiguration]
        public SampleConfigurationWithDefaultValues Config { get; set; }

        public void OnImportsSatisfied()
        {
            OnImportsSatisfiedCalled++;
        }
    }

    [PluginConfiguration]
    public class SampleConfigurationWithDefaultValues
    {
        public int Value { get; set; }
        public int SecondValue { get; set; }
        public SampleConfigurationWithDefaultValues()
        {
            Value = 5;
            SecondValue = 50;
        }
    }
}