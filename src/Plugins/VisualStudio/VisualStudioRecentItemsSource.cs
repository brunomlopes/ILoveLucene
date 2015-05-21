using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using Core.API;
using Microsoft.Win32;

namespace Plugins.VisualStudio
{
    [Export(typeof(IItemSource))]
    public class VisualStudioRecentItemsSource : BaseItemSource
    {
        public override IEnumerable<object> GetItems()
        {
            var versionsKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\VisualStudio\");

            foreach (var subKeyName in versionsKey?.GetSubKeyNames() ?? new string[] { })
            {
                var subKey = versionsKey.OpenSubKey(subKeyName);
                var projectMruKey = subKey?.OpenSubKey("ProjectMRUList");

                foreach (var projectKey in projectMruKey?.GetValueNames() ?? new string[] { })
                {
                    var mruInfo = projectMruKey.GetValue(projectKey).ToString();
                    var path = mruInfo.Split('|')[0];
                    var fileInfo = new FileInfo(path);
                    if (fileInfo.Exists) yield return fileInfo;
                }
            }
        }
    }
}