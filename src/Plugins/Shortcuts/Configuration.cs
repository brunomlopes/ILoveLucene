using System.Collections.Generic;
using Core.API;
using Core.Abstractions;

namespace Plugins.Shortcuts
{
    [PluginConfiguration]
    public class Configuration
    {
        public List<string> Directories { get; set; }
        public List<string> Extensions { get; set; }
    }
}