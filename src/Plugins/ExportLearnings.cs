using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Text;
using Core.Lucene;
using Newtonsoft.Json;
using Plugins.Commands;

namespace Plugins
{
    //[Export(typeof(ICommand))]
    //public class ExportLearnings : BaseCommand
    //{
    //    public override void Act()
    //    {
    //        var luceneBase = new LuceneBase();
    //        var indexReader = luceneBase.GetReadOnlyIndexReader();
    //        var learnings = new Dictionary<string, object>();

    //        for (int i = 0; i < indexReader.MaxDoc(); i++)
    //        {
    //            if(indexReader.IsDeleted(i)) continue;

    //            var doc = indexReader.Document(i);
    //            var sha1 = doc.GetField(SpecialFields.Sha1);
    //            var docLearnings = doc.GetField(SpecialFields.Learnings);
    //            if (sha1 != null && docLearnings != null)
    //            {
    //                if(string.IsNullOrWhiteSpace(sha1.StringValue()))
    //                    continue;
    //                if(string.IsNullOrWhiteSpace(docLearnings.StringValue()))
    //                    continue;

    //                learnings[sha1.StringValue()] = new
    //                                                    {
    //                                                        Id = doc.GetField(SpecialFields.Id).StringValue(),
    //                                                        Namespace = doc.GetField(SpecialFields.Namespace).StringValue(),
    //                                                        Name = doc.GetField(SpecialFields.Name).StringValue(),
    //                                                        Learnings = docLearnings.StringValue(),
    //                                                    };
    //            }
    //        }

    //        File.WriteAllText("learnings.json", JsonConvert.SerializeObject(learnings, Formatting.Indented), Encoding.UTF8);
    //    }

    //    public override string Description
    //    {
    //        get { return "Exports all learnings to a file named 'learnings.json'"; }
    //    }
    //}
}