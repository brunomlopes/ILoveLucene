using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Core.Abstractions;
using Lucene.Net.Search;

namespace ILoveLucene.Views
{
    /// <summary>
    /// Interaction logic for Explanation.xaml
    /// </summary>
    public partial class ExplanationView : Window
    {
        public ExplanationView(IEnumerable<AutoCompletionResult.CommandResult> commandResults)
        {
            InitializeComponent();

            foreach (var commandResult in commandResults)
            {
                var commandTreeItem = new TreeViewItem {Header = commandResult.Item.Text + commandResult.CompletionId};
                Action<TreeViewItem, Explanation> renderExplanation = null;
                renderExplanation = (t, exp) =>
                                        {
                                            var header = exp.Value + " " + exp.Description + " ";
                                            var childItem = new TreeViewItem {Header = header};
                                            t.Items.Add(childItem);
                                            foreach (var explanation in exp.GetDetails() ?? new Explanation[] {})
                                            {
                                                renderExplanation(childItem, explanation);
                                            }
                                        };
                renderExplanation(commandTreeItem, commandResult.Explanation);
                commandTreeItem.ExpandSubtree();
                ExplanationTree.Items.Add(commandTreeItem); 
            }
        }
    }
}
