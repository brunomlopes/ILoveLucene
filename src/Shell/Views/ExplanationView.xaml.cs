using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
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
            StringBuilder builder = new StringBuilder();

            foreach (var commandResult in commandResults)
            {
                builder.AppendLine(commandResult.Item.Text + commandResult.CompletionId);
                Action<string, Explanation> renderExplanation = null;
                renderExplanation = (i, exp) =>
                                        {
                                            builder.AppendLine(i + + exp.GetValue() + " "+ exp.GetDescription());
                                            foreach (var explanation in exp.GetDetails() ?? new Explanation[] {})
                                            {
                                                renderExplanation(i + " ", explanation);
                                            }
                                        };
                renderExplanation(" ", commandResult.Explanation);
            }
            Explanation.Text = builder.ToString();
        }
    }
}
