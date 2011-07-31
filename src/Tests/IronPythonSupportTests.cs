using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using Core.Abstractions;
using IronPython.Hosting;
using Plugins.IronPython;
using Xunit;
using Xunit.Extensions;

namespace Tests
{
    public class IronPythonSupportTests :TestClass
    {
        [Fact]
        public void ExtractTokensFromString()
        {
            var pythonCode =
                @"
class StringItemSource(BasePythonItemSource):
    def GetAllItems(self):
        return [""Item 1"", ""Item 2"", ""Item 3""]
";

            var _engine = Python.CreateEngine();
            var script = _engine.CreateScriptSourceFromString(pythonCode);
            var typeExtractor = new ExtractTypesFromScript(_engine);
            var types = typeExtractor.GetTypesFromScript(script).ToList();
            Assert.Equal(1, types.Count());
            Assert.Equal("StringItemSource", types.First().Name);

            var instance = types.First().Activator();
            Assert.IsAssignableFrom<BasePythonItemSource>(instance);
            Assert.IsAssignableFrom<IItemSource>(instance);
        }

        [Fact]
        public void CanComposeExportsFromPythonCode()
        {
            var pythonCode =
                @"
class StringItemSource(BasePythonItemSource):
    __exports__ = [IItemSource]

    def GetAllItems(self):
        return [""Item 1"", ""Item 2"", ""Item 3""]
";

            var _engine = Python.CreateEngine();
            var script = _engine.CreateScriptSourceFromString(pythonCode);
            var typeExtractor = new ExtractTypesFromScript(_engine);
            var types = typeExtractor.GetTypesFromScript(script).ToList();

            var createExports = new CreateExportsFromTypes();
            var exports = createExports.GetExports(types).ToList();

            var container = new CompositionContainer();
            var batch = new CompositionBatch(exports, new ComposablePart[] {});
            container.Compose(batch);

            var instance = new MockImporter();
            container.SatisfyImportsOnce(instance);

            Assert.Equal(1, instance.ItemSources.Count());
        }
        
        [Fact]
        public void ClassWithoutExportsResultsInZeroParts()
        {
            var pythonCode =
                @"
class StringItemSource(BasePythonItemSource):
    def GetAllItems(self):
        return [""Item 1"", ""Item 2"", ""Item 3""]
";

            var _engine = Python.CreateEngine();
            var script = _engine.CreateScriptSourceFromString(pythonCode);
            var typeExtractor = new ExtractTypesFromScript(_engine);
            var types = typeExtractor.GetTypesFromScript(script).ToList();

            var createExports = new CreateExportsFromTypes();
            var exports = createExports.GetExports(types).ToList();
            Assert.Equal(0, exports.Count());
        }
    }

    public class MockImporter
    {
        [ImportMany]
        public IEnumerable<IItemSource> ItemSources { get; set; }
    }
}