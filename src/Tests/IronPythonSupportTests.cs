using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Threading.Tasks;
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
            var exports = typeExtractor.GetPartsFromScript(script).ToList();

            var container = new CompositionContainer();
            var batch = new CompositionBatch(exports, new ComposablePart[] {});
            container.Compose(batch);

            var instance = new MockImporter();
            container.SatisfyImportsOnce(instance);

            Assert.Equal(1, instance.ItemSources.Count());
        }
        
        [Fact]
        public void CanImportListIntoPythonClass()
        {
            var pythonCode =
                @"
class StringItemSource:
    def import_actions(self, actions):
        self.actions = actions
    def normal_method(self):
        pass

StringItemSource.import_actions.func_dict['imports'] = IronPythonImportDefinition('import_action', IActOnItem, 'ZeroOrOne', True, True)
";

            var _engine = Python.CreateEngine();
            var script = _engine.CreateScriptSourceFromString(pythonCode);
            
            var typeExtractor = new ExtractTypesFromScript(_engine);
            var exports = typeExtractor.GetPartsFromScript(script).ToList();

            var container = new CompositionContainer(new TypeCatalog(typeof(MockExporter), typeof(MockImportActions)));
            
            var batch = new CompositionBatch(exports, new ComposablePart[] {});
            
            container.Compose(batch);

            var value = container.GetExportedValue<MockImportActions>();
            Assert.Equal(1, value.ActOnItems.Count());
            IEnumerable actions = exports.First().Instance.actions;
            Assert.Equal(1, actions.OfType<IActOnItem>().Count());
            Assert.Equal(1, actions.OfType<MockExporter>().Count());
        }
        
        [Fact]
        public void CanImportIntoPythonClassUsingDecorator()
        {
            var pythonCode =
                @"
class StringItemSource:
    @import_many(IActOnItem)
    def import_actions(self, actions):
        self.actions = actions
";

            var _engine = Python.CreateEngine();
            var paths = _engine.GetSearchPaths();
            paths.Add(@"D:\documents\dev\ILoveLucene\lib\ironpython\Lib");
            _engine.SetSearchPaths(paths);
            var script = _engine.CreateScriptSourceFromString(pythonCode);
            
            var typeExtractor = new ExtractTypesFromScript(_engine);
            var exports = typeExtractor.GetPartsFromScript(script).ToList();

            var container = new CompositionContainer(new TypeCatalog(typeof(MockExporter), typeof(MockImportActions)));
            
            var batch = new CompositionBatch(exports, new ComposablePart[] {});
            
            container.Compose(batch);

            var value = container.GetExportedValue<MockImportActions>();
            Assert.Equal(1, value.ActOnItems.Count());
            IEnumerable actions = exports.First().Instance.actions;
            Assert.Equal(1, actions.OfType<IActOnItem>().Count());
            Assert.Equal(1, actions.OfType<MockExporter>().Count());
        }
        
        [Fact]
        public void CanImportJustOneItemIntoPythonClassUsingDecorator()
        {
            var pythonCode =
                @"
class StringItemSource:
    @import_one(IActOnItem)
    def import_action(self, action):
        self.action = action
";

            var _engine = Python.CreateEngine();
            var paths = _engine.GetSearchPaths();
            paths.Add(@"D:\documents\dev\ILoveLucene\lib\ironpython\Lib");
            _engine.SetSearchPaths(paths);
            var script = _engine.CreateScriptSourceFromString(pythonCode);
            
            var typeExtractor = new ExtractTypesFromScript(_engine);
            var exports = typeExtractor.GetPartsFromScript(script).ToList();

            var container = new CompositionContainer(new TypeCatalog(typeof(MockExporter), typeof(MockImportActions)));
            
            var batch = new CompositionBatch(exports, new ComposablePart[] {});
            
            container.Compose(batch);

            object action = exports.First().Instance.action;
            Assert.NotNull(action);
            Assert.IsAssignableFrom<IActOnItem>(action);
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
            var exports = typeExtractor.GetPartsFromScript(script).ToList();

            Assert.Equal(0, exports.Count());
        }
    }

    public class MockImporter
    {
        [ImportMany]
        public IEnumerable<IItemSource> ItemSources { get; set; }
    }

    [Export(typeof(MockImportActions))]
    public class MockImportActions
    {
        [ImportMany]
        public IEnumerable<IActOnItem> ActOnItems { get; set; }
    }

    [Export(typeof(IActOnItem))]
    public class MockExporter : IActOnItem
    {
        public string Text
        {
            get { return "Act"; }
        }

        public Type TypedItemType
        {
            get { return typeof(string); }
        }
    }
}