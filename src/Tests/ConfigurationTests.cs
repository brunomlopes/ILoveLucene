using System.ComponentModel.Composition.Hosting;
using System.IO;
using Core;
using Core.Abstractions;
using Newtonsoft.Json;
using Plugins.Shortcuts;
using Xunit;

namespace Tests
{
    public class ConfigurationTests
    {
        [Fact]
        public void CanLoadShortcutConfiguration()
        {
            var container = new CompositionContainer();
            var configurationCatalog = new LoadConfiguration(new DirectoryInfo(@"..\..\..\Shell\bin\Debug\Configuration"));
            configurationCatalog.Load(container);

            var conf = container.GetExportedValue<Configuration>();
            Assert.Contains(@"%USERPROFILE%\AppData\Roaming\Microsoft\Windows\Start Menu", conf.Directories);
        }
        
        [Fact]
        public void ReloadingConfigurationShouldNotDuplicateEntries()
        {
            var container = new CompositionContainer();
            var configurationCatalog = new LoadConfiguration(new DirectoryInfo(@"..\..\..\Shell\bin\Debug\Configuration"));
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
        public void RequestingStoredConfigurationWithADefaultTypeAlreadyRegisteredShouldReturnStoredConfiguration()
        {
            var configurationDirectory = new DirectoryInfo(@"temp-configuration");
            if (configurationDirectory.Exists)
            {
                configurationDirectory.Delete(true);
            }
            configurationDirectory.Create();
            var conf = new SampleConfigurationWithDefaultValues() {Value = 10};
            File.WriteAllText(Path.Combine(configurationDirectory.FullName, typeof(SampleConfigurationWithDefaultValues).AssemblyQualifiedName), JsonConvert.SerializeObject(conf));


            var container = new CompositionContainer(new AssemblyCatalog(this.GetType().Assembly));
            var configurationCatalog = new LoadConfiguration(configurationDirectory);
            configurationCatalog.Load(container);

            conf = container.GetExportedValue<SampleConfigurationWithDefaultValues>();
            Assert.Equal(10, conf.Value);
        }
        
        [Fact(Skip = "Not able to compose twice :S")]
        public void ComposingTwiceIsNotABigDeal()
        {
            var container = new CompositionContainer();
            var configurationCatalog = new LoadConfiguration(new DirectoryInfo(@"..\..\..\Shell\bin\Debug\Configuration"));
            configurationCatalog.Load(container);

            var conf = container.GetExportedValue<Configuration>();
            Assert.Contains(@"%USERPROFILE%\AppData\Roaming\Microsoft\Windows\Start Menu", conf.Directories);

            configurationCatalog.Load(container);
            conf = container.GetExportedValue<Configuration>();
            Assert.Contains(@"%USERPROFILE%\AppData\Roaming\Microsoft\Windows\Start Menu", conf.Directories);
        }
    }


    [PluginConfiguration]
    public class SampleConfigurationWithDefaultValues
    {
        public int Value { get; set; }
        public SampleConfigurationWithDefaultValues()
        {
            Value = 5;
        }
    }
}