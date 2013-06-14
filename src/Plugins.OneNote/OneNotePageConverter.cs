using System.ComponentModel.Composition;
using Core.API;

namespace Plugins.OneNote
{
    [Export(typeof (IConverter))]
    public class OneNotePageConverter : IConverter<OneNotePage>
    {
        public CoreDocument ToDocument(IItemSource itemSource, OneNotePage t)
        {
            var coreDoc = new CoreDocument(itemSource, this, t.Id, t.SectionName + " - " + t.Name, "onenotepage");

            coreDoc.Store("id", t.Id)
                   .Store("name", t.Name)
                   .Store("sectionnodepath", t.SectionNodePath)
                   .Store("sectionname", t.SectionName);

            return coreDoc;
        }

        public IItem FromDocumentToItem(CoreDocument document)
        {
            var page = new OneNotePage()
                {
                    Id = document.GetString("id"),
                    Name = document.GetString("name"),
                    SectionNodePath = document.GetString("sectionnodepath"),
                    SectionName = document.GetString("sectionname")
                };
            return page;
        }
    }
}