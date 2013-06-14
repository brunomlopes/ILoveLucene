using System.ComponentModel.Composition;
using System.Diagnostics;
using Core.API;

namespace Plugins.OneNote
{
    [Export(typeof (IActOnItem))]
    public class OpenOneNotePage : BaseActOnTypedItem<OneNotePage>
    {
        public override void ActOn(OneNotePage item)
        {
            var url = string.Format("onenote:{0}#{1}",
                                    item.SectionNodePath.Replace(" ", "%20"),
                                    item.Name.Replace(" ", "%20"));


            var info = new ProcessStartInfo("cmd", "/c start /b " + url)
                {
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
            Process.Start(info);
        }
    }
}