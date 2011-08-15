using System;
using System.Collections.Generic;
using System.Xml;
using NAppUpdate.Framework.Conditions;
using NAppUpdate.Framework.FeedReaders;
using NAppUpdate.Framework.Tasks;

namespace ILoveLucene.AutoUpdate
{
    public class ZippedAppcastReader : IUpdateFeedReader
    {
        private readonly ModuleVersionRegistry _registry;

        public ZippedAppcastReader(ModuleVersionRegistry registry)
        {
            _registry = registry;
        }

        // http://learn.adobe.com/wiki/display/ADCdocs/Appcasting+RSS

        public IList<IUpdateTask> Read(string feed)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(feed);
            XmlNodeList nl = doc.SelectNodes("/rss/channel/item");

            List<IUpdateTask> ret = new List<IUpdateTask>();

            foreach (XmlNode n in nl)
            {
                ZippedFilesUpdateTask task = new ZippedFilesUpdateTask();
                if(n["warm-update"] != null && n["warm-update"].InnerText.ToLowerInvariant() == "true")
                {
                    task.Attributes.Add("warm-update", "true");
                }
                task.Description = n["description"].InnerText;
                string remotePath = n["enclosure"].Attributes["url"].Value;

                task.Attributes.Add("remotePath", remotePath);

                if(!remotePath.ToLowerInvariant().EndsWith(".zip"))
                {
                    throw new InvalidOperationException(
                        "Appcast feed contains urls which are not zip files. That isn't supported by this reader");
                }

                var version = _registry.VersionForCoreModule();
                if(n["appcast:module"] != null)
                {
                    version = _registry.VersionForModule(n["appcast:module"].InnerText);
                }
                var cnd = new VersionCondition(version);
                cnd.Attributes.Add("version", n["appcast:version"].InnerText);
                task.UpdateConditions.AddCondition(cnd, BooleanCondition.ConditionType.AND);

                ret.Add(task);
            }

            return ret;
        }
    }
}
