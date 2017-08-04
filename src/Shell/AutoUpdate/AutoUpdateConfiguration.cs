using Core.API;
using Core.Abstractions;

namespace ILoveLucene.AutoUpdate
{
    [PluginConfiguration]
    public class AutoUpdateConfiguration 
    {
        public string AppcastFeedUrl { get; set; }
        public bool CheckForUpdates { get; set; }
        public int PeriodicityInMinutes { get; set; }

        public AutoUpdateConfiguration()
        {
            AppcastFeedUrl = "https://brunomlopeswe.blob.core.windows.net/brunomlopes-ilovelucene/appcast.xml";
            CheckForUpdates = true;
            PeriodicityInMinutes = 30;
        }
    }
}