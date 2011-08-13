using System.IO;

namespace Tests.Helpers
{
    public static class PathHelper
    {
        public static DirectoryInfo AsNewDirectoryInfo(this string path)
        {
            var configurationDirectory = new DirectoryInfo(path);
            if (configurationDirectory.Exists)
            {
                configurationDirectory.Delete(true);
            }
            configurationDirectory.Create();
            return configurationDirectory;
        }
        
        public static void WriteToFileInPath(this string content, DirectoryInfo path, string filename)
        {
            File.WriteAllText(Path.Combine(path.FullName, filename), content);
        }
    }
}