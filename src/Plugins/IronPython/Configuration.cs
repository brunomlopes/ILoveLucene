using System.Collections.Generic;
using Core.Abstractions;

namespace Plugins.IronPython
{
    [PluginConfiguration]
    public class Configuration
    {
        public List<string> ScriptDirectories { get; set; }

        public Configuration()
        {
            ScriptDirectories = new List<string>() {"$plugins$\\IronPython"};
        }
    }
}