using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
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
            var configurationCatalog = new LoadConfiguration(new DirectoryInfo(_PathToDebugConfiguration));
            configurationCatalog.Load(container);

            var conf = container.GetExportedValue<Configuration>();
            Assert.Contains(@"%USERPROFILE%\AppData\Roaming\Microsoft\Windows\Start Menu", conf.Directories);
        }
        
        [Fact]
        public void ReloadingConfigurationShouldNotDuplicateEntries()
        {
            var container = new CompositionContainer();
            var configurationCatalog = new LoadConfiguration(new DirectoryInfo(_PathToDebugConfiguration));
            configurationCatalog.Load(container);

            var conf = container.GetExportedValue<Configuration>();
            var count = conf.Directories.Count;
            configurationCatalog.Reload();
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

            var configurationCatalog = new LoadConfiguration(configurationDirectory);
            configurationCatalog.Load(container);

            var conf = container.GetExportedValue<SampleConfigurationWithDefaultValues>();
            Assert.Equal(5, conf.Value);
        }
        
        [Fact]
        public void ShouldIgnoreReadmeFile()
        {
            var container = new CompositionContainer(new AssemblyCatalog(this.GetType().Assembly));

            var configurationDirectory = new DirectoryInfo(@"temp-configuration");
            if (configurationDirectory.Exists)
            {
                configurationDirectory.Delete(true);
            }
            configurationDirectory.Create();
            File.WriteAllText(Path.Combine(configurationDirectory.FullName,"readme.txt"), "Text");
            var configurationCatalog = new LoadConfiguration(configurationDirectory);
            configurationCatalog.Load(container);

            var conf = container.GetExportedValue<SampleConfigurationWithDefaultValues>();
            Assert.Equal(5, conf.Value);
        }

        [Fact]
        public void RequestingStoredConfigurationWithADefaultTypeAlreadyRegisteredShouldReturnStoredConfiguration()
        {
            var configurationDirectory = new DirectoryInfo(@"temp-configuration");
            if (configurationDirectory.Exists)
            {
                configurationDirectory.Delete(true);
            }
            configurationDirectory.Create();
            var conf = new SampleConfigurationWithDefaultValues() {Value = 10};
            WriteConfiguration<SampleConfigurationWithDefaultValues>(configurationDirectory, conf);

            var container = new CompositionContainer(new AssemblyCatalog(this.GetType().Assembly));
            var configurationCatalog = new LoadConfiguration(configurationDirectory);
            configurationCatalog.Load(container);

            conf = container.GetExportedValue<SampleConfigurationWithDefaultValues>();
            Assert.Equal(10, conf.Value);
        }

        [Fact]
        public void CanLoadConfigurationFromGlobalAndUserRepository()
        {
            var userConfigurationDirectory = new DirectoryInfo("user-configuration");
            var systemConfigurationDirectory = new DirectoryInfo("temp-configuration");

            if(userConfigurationDirectory.Exists)
            {
                userConfigurationDirectory.Delete(true);
            }
            userConfigurationDirectory.Create();
            if(systemConfigurationDirectory.Exists)
            {
                systemConfigurationDirectory.Delete(true);
            }
            systemConfigurationDirectory.Create();

            var systemConfiguration = new SampleConfigurationWithDefaultValues() {Value = 10, SecondValue = 200};
            var userConfiguration = new {Value = 20};
            WriteConfiguration<SampleConfigurationWithDefaultValues>(userConfigurationDirectory, userConfiguration);
            WriteConfiguration<SampleConfigurationWithDefaultValues>(systemConfigurationDirectory, systemConfiguration);

            var container = new CompositionContainer();
            var configurationCatalog = new LoadConfiguration(systemConfigurationDirectory);
            configurationCatalog.AddConfigurationLocation(userConfigurationDirectory);

            configurationCatalog.Load(container);
            var conf = container.GetExportedValue<SampleConfigurationWithDefaultValues>();
            Assert.Equal(20, conf.Value);
            Assert.Equal(200, conf.SecondValue);
        }
        
        [Fact]
        public void CanLoadConfigurationFromGlobalAndUserRepositoryUsingShortName()
        {
            var userConfigurationDirectory = new DirectoryInfo("user-configuration");
            var systemConfigurationDirectory = new DirectoryInfo("temp-configuration");

            if(userConfigurationDirectory.Exists)
            {
                userConfigurationDirectory.Delete(true);
            }
            userConfigurationDirectory.Create();
            if(systemConfigurationDirectory.Exists)
            {
                systemConfigurationDirectory.Delete(true);
            }
            systemConfigurationDirectory.Create();

            var systemConfiguration = new SampleConfigurationWithDefaultValues() {Value = 10, SecondValue = 200};
            var userConfiguration = new {Value = 20};
            WriteConfiguration<SampleConfigurationWithDefaultValues>(userConfigurationDirectory, userConfiguration, true);
            WriteConfiguration<SampleConfigurationWithDefaultValues>(systemConfigurationDirectory, systemConfiguration, true);

            var container = new CompositionContainer();
            var configurationCatalog = new LoadConfiguration(systemConfigurationDirectory);
            configurationCatalog.AddConfigurationLocation(userConfigurationDirectory);

            configurationCatalog.Load(container);
            var conf = container.GetExportedValue<SampleConfigurationWithDefaultValues>();
            Assert.Equal(20, conf.Value);
            Assert.Equal(200, conf.SecondValue);
        }

        [Fact]
        public void CanLoadConfigurationFromGlobalAndUserRepositoryUsingShortAndLongName()
        {
            var userConfigurationDirectory = new DirectoryInfo("user-configuration");
            var systemConfigurationDirectory = new DirectoryInfo("temp-configuration");

            if(userConfigurationDirectory.Exists)
            {
                userConfigurationDirectory.Delete(true);
            }
            userConfigurationDirectory.Create();
            if(systemConfigurationDirectory.Exists)
            {
                systemConfigurationDirectory.Delete(true);
            }
            systemConfigurationDirectory.Create();

            var systemConfiguration = new SampleConfigurationWithDefaultValues() {Value = 10, SecondValue = 200};
            var userConfiguration = new {Value = 20};
            WriteConfiguration<SampleConfigurationWithDefaultValues>(userConfigurationDirectory, userConfiguration, true);
            WriteConfiguration<SampleConfigurationWithDefaultValues>(systemConfigurationDirectory, systemConfiguration);

            var container = new CompositionContainer();
            var configurationCatalog = new LoadConfiguration(systemConfigurationDirectory);
            configurationCatalog.AddConfigurationLocation(userConfigurationDirectory);

            configurationCatalog.Load(container);
            var conf = container.GetExportedValue<SampleConfigurationWithDefaultValues>();
            Assert.Equal(20, conf.Value);
            Assert.Equal(200, conf.SecondValue);
        }

        [Fact(Skip = "Not able to compose twice :S")]
        public void ComposingTwiceIsNotABigDeal()
        {
            var container = new CompositionContainer();
            var configurationCatalog = new LoadConfiguration(new DirectoryInfo(_PathToDebugConfiguration));
            configurationCatalog.Load(container);

            var conf = container.GetExportedValue<Configuration>();
            Assert.Contains(@"%USERPROFILE%\AppData\Roaming\Microsoft\Windows\Start Menu", conf.Directories);

            configurationCatalog.Load(container);
            conf = container.GetExportedValue<Configuration>();
            Assert.Contains(@"%USERPROFILE%\AppData\Roaming\Microsoft\Windows\Start Menu", conf.Directories);
        }

        private void WriteConfiguration<T>(DirectoryInfo configurationDirectory, object conf,
                                           bool useShortTypeName = false)
        {
            string fileName = typeof(T).AssemblyQualifiedName;
            if(useShortTypeName)
            {
                fileName = string.Join(",", fileName.Split(',').Take(2).ToArray());
            }
            string filePath = Path.Combine(configurationDirectory.FullName,
                                          fileName);
            File.WriteAllText(
                filePath,
                JsonConvert.SerializeObject(conf));
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