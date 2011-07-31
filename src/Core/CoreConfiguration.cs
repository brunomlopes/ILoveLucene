using System.IO;
using System.Reflection;

namespace Core
{
    public class CoreConfiguration
    {
        public string DataDirectory { get; private set; }
        public string PluginsDirectory { get; private set; }

        public CoreConfiguration(string dataDirectory, string pluginsDirectory)
        {
            DataDirectory = dataDirectory;
            PluginsDirectory = pluginsDirectory;

            if (!Directory.Exists(DataDirectory))
            {
                Directory.CreateDirectory(dataDirectory);
            }
        }
    }
}