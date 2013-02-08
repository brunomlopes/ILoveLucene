using System;
using System.IO;
using Core.API;
using Core.Abstractions;

namespace Plugins.Calibre
{
    [PluginConfiguration]
    public class Configuration
    {
        public string PathToCalibreInstalation { get; set; }
        public int MaximumSecondsProcessShouldTake { get; set; }
        public Configuration()
        {
            var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            PathToCalibreInstalation = Path.Combine(programFiles, "Calibre2");

            if (!Directory.Exists(PathToCalibreInstalation))
            {
                programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            }
            PathToCalibreInstalation = Path.Combine(programFiles, "Calibre2");


            MaximumSecondsProcessShouldTake = 600;
        }
    }
}