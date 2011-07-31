using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using Core.Abstractions;
using Core.Extensions;
using IronPython.Hosting;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Util;
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
            var a = new TypeExtractor(_engine);
            var types = a.GetTypesFromScript(script).ToList();
            Assert.Equal(1, types.Count());
            Assert.Equal("StringItemSource", types.First().Name);

            var instance = types.First().Activator();
            Assert.IsAssignableFrom<BasePythonItemSource>(instance);
            Assert.IsAssignableFrom<IItemSource>(instance);
        }
    }
}