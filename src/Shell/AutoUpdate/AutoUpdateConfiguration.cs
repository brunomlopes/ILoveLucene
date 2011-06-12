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
            AppcastFeedUrl = "http://dl.dropbox.com/u/118385/ilovelucene/appcast.xml";
            CheckForUpdates = true;
            PeriodicityInMinutes = 30;
        }
    }
}