using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Core.API;
using Microsoft.Office.Interop.OneNote;

namespace Plugins.OneNote
{
    [Export(typeof (IItemSource))]
    public class OneNotePageSource : BaseItemSource
    {
        public override Task<IEnumerable<object>> GetItems()
        {
            return Task.Factory.StartNew(() => GetPages());
        }

        private IEnumerable<object> GetPages()
        {
            var onenoteApp = new Application();

            string notebookXml;
            onenoteApp.GetHierarchy(null, HierarchyScope.hsPages, out notebookXml);

            var doc = XDocument.Parse(notebookXml);
            var ns = doc.Root.Name.Namespace;
            foreach (var notebookNode in
                doc.Descendants(ns + "Notebook").Select(node => node))
            {
                foreach (var sectionNode in notebookNode.Descendants(ns + "Section").Select(node => node))
                {
                    foreach (var pageNode in sectionNode.Descendants(ns + "Page").Select(node => node))
                    {
                        yield return new OneNotePage()
                            {
                                Id = pageNode.Attribute("ID").Value,
                                Name = pageNode.Attribute("name").Value,
                                SectionNodePath = sectionNode.Attribute("path").Value,
                                SectionName = sectionNode.Attribute("name").Value
                            };
                    }
                }
            }
        }
    }
}