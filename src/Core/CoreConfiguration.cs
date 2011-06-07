using System.IO;
using System.Reflection;

namespace Core
{
    public class CoreConfiguration
    {
        public string DataDirectory { get; private set; }
        public CoreConfiguration(string dataDirectory)
        {
            DataDirectory = dataDirectory;
            if (!Directory.Exists(DataDirectory))
            {
                Directory.CreateDirectory(dataDirectory);
            }
        }
    }
}