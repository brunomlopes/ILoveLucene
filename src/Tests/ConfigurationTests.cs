using System.ComponentModel.Composition.Hosting;
using System.IO;
using Core;
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
            var configurationCatalog = new ConfigurationComposer(new DirectoryInfo(@"..\..\..\Shell\bin\Debug\Configuration"));
            configurationCatalog.Compose(container);

            var conf = container.GetExportedValue<Configuration>();
            Assert.Contains(@"%USERPROFILE%\AppData\Roaming\Microsoft\Windows\Start Menu", conf.Directories);
        }
        
        [Fact(Skip = "Not able to compose twice :S")]
        public void ComposingTwiceIsNotABigDeal()
        {
            var container = new CompositionContainer();
            var configurationCatalog = new ConfigurationComposer(new DirectoryInfo(@"..\..\..\Shell\bin\Debug\Configuration"));
            configurationCatalog.Compose(container);

            var conf = container.GetExportedValue<Configuration>();
            Assert.Contains(@"%USERPROFILE%\AppData\Roaming\Microsoft\Windows\Start Menu", conf.Directories);

            configurationCatalog.Compose(container);
            conf = container.GetExportedValue<Configuration>();
            Assert.Contains(@"%USERPROFILE%\AppData\Roaming\Microsoft\Windows\Start Menu", conf.Directories);
        }
    }
}