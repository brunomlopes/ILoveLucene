using System;
using System.Collections.Generic;
using System.Xml;
using NAppUpdate.Framework.Conditions;
using NAppUpdate.Framework.FeedReaders;
using NAppUpdate.Framework.Tasks;
using System.Linq;

namespace ILoveLucene.AutoUpdate
{
    public class ZippedAppcastReader : IUpdateFeedReader
    {
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
                task.Description = n["description"].InnerText;
                string remotePath = n["enclosure"].Attributes["url"].Value;

                task.Attributes.Add("remotePath", remotePath);

                if(!remotePath.ToLowerInvariant().EndsWith(".zip"))
                {
                    throw new InvalidOperationException(
                        "Appcast feed contains urls which are not zip files. That isn't supported by this reader");
                }

                ProgramVersionCondition cnd = new ProgramVersionCondition();
                cnd.Attributes.Add("version", n["appcast:version"].InnerText);
                task.UpdateConditions.AddCondition(cnd, BooleanCondition.ConditionType.AND);

                ret.Add(task);
            }

            return ret;
        }
    }
}
