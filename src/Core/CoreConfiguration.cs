using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Linq;

namespace Core
{
    public class CoreConfiguration
    {
        public string DataDirectory { get; private set; }
        public string PluginsDirectory { get; private set; }

        public const string PluginsPlaceholder = "$plugins$";
        public const string UserDataPlaceholder = "$user_data$";

        public CoreConfiguration(string dataDirectory, string pluginsDirectory)
        {
            DataDirectory = dataDirectory;
            PluginsDirectory = pluginsDirectory;

            if (!Directory.Exists(DataDirectory))
            {
                Directory.CreateDirectory(dataDirectory);
            }
        }

        public IEnumerable<string> ExpandPaths(IEnumerable<string> paths)
        {
            var data = DataDirectory.EndsWith("\\") ? DataDirectory.TrimEnd('\\') : DataDirectory;
            var plugins = PluginsDirectory.EndsWith("\\") ? PluginsDirectory.TrimEnd('\\') : PluginsDirectory;
            return paths.Select(s => s.Replace(UserDataPlaceholder, data).Replace(PluginsPlaceholder, plugins)).ToList();
        }
    }
}