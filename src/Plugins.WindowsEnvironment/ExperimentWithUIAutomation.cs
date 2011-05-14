using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Automation;

namespace Plugins.WindowsEnvironment
{
    public static class AutomationElementExtensions
    {
        public static object ValueOn(this AutomationProperty property, AutomationElement element)
        {
            return element.GetCurrentPropertyValue(property);
        }
        public static ControlType ControlType(this AutomationElement element)
        {
            return (ControlType) AutomationElement.ControlTypeProperty.ValueOn(element);
            
        }
        
        public static string Name(this AutomationElement element)
        {
            return AutomationElement.NameProperty.ValueOn(element).ToString();
            
        }
        
        public static bool IsSelectionPatternAvailable(this AutomationElement element)
        {
            return (bool) AutomationElement.IsSelectionPatternAvailableProperty.ValueOn(element);
        }

        public static SelectionPattern GetSelectionPattern(this AutomationElement element)
        {
            var selectionPattern = (SelectionPattern) element.GetCurrentPattern(SelectionPattern.Pattern);
            return  selectionPattern;
        }

        public static AutomationElement[] GetSelectedItems(this AutomationElement element)
        {
            if(!element.IsSelectionPatternAvailable())
            {
                return new AutomationElement[]{};
            }
            return element.GetSelectionPattern().Current.GetSelection();
        }
        public static bool HasSelectedItems(this AutomationElement element)
        {
            return element.GetSelectedItems().Length > 0;
        }

        public static SelectionItemPattern GetSelectionItemPattern(this AutomationElement element)
        {
            var p = (SelectionItemPattern) element.GetCurrentPattern(SelectionItemPattern.Pattern);
            return p;
        }
        public static bool IsSelected(this AutomationElement element)
        {
            if (!element.IsSelectionPatternAvailable())
                return false;

            var p = (SelectionItemPattern) element.GetCurrentPattern(SelectionItemPattern.Pattern);
            return p.Current.IsSelected;
        }

        public static object GetFiles(this AutomationElement element)
        {
            ValuePattern pattern = (ValuePattern) element.GetCurrentPattern(ValuePattern.Pattern);
            return pattern.Current.Value;
        }

    }
    public class ExperimentWithUIAutomation
    {
        public ExperimentWithUIAutomation()
        {
            var windows = AutomationElement.RootElement.FindAll(TreeScope.Children,
                                                                new PropertyCondition(
                                                                    AutomationElement.ControlTypeProperty,
                                                                    ControlType.Window));

            var explorerWindows = windows.OfType<AutomationElement>()
                .Where(w => ProcessNameForElement(w).ToLowerInvariant().Equals("explorer"));

            foreach(var element in explorerWindows)
            {
                Dictionary<AutomationElement, int> levels = new Dictionary<AutomationElement, int>();
                WalkControlElements(element, 0, (level, parent, child) =>
                                                    {
                                                        if(!child.HasSelectedItems()) return;
                                                        Debug.WriteLine(new String(' ', level) +
                                                                        AutomationElement.NameProperty.ValueOn(parent).
                                                                            ToString() +
                                                                        "->" +
                                                                        AutomationElement.NameProperty.ValueOn(child).
                                                                            ToString() +"("+
                                                                        SelectedForElement(child)+")");
                                                    });
            }
        }
        private string SelectedForElement(AutomationElement element)
        {
            var items = element.GetSelectedItems();
            if (items.Length == 0)
                return string.Empty;
            var sb = new StringBuilder();
            foreach (var automationElement in items)
            {
                sb.AppendLine(automationElement.Name());
                sb.AppendLine("-Properties");
                foreach (var automationProperty in automationElement.GetSupportedProperties())
                {
                    object value = automationElement.GetCurrentPropertyValue(automationProperty);
                    sb.AppendLine("\t"+automationProperty.ProgrammaticName+"-"+value);
                }
                sb.AppendLine("-Patterns");
                foreach (var automationPattern in automationElement.GetSupportedPatterns())
                {
                    object currentPattern = automationElement.GetCurrentPattern(automationPattern);
                    sb.AppendLine("\t"+automationPattern.ProgrammaticName+"-"+currentPattern);
                }

                if(automationElement.GetSupportedPatterns().Contains(InvokePattern.Pattern))
                {
                    ValuePattern pat = (ValuePattern)automationElement.GetCurrentPattern(ValuePattern.Pattern);
                    
                }
            }
            return sb.ToString();
        }
        private string ProcessNameForElement(AutomationElement w)
        {
            return Process.GetProcessById((int)w.GetCurrentPropertyValue(AutomationElement.ProcessIdProperty)).ProcessName;
        }

        private void WalkControlElements(AutomationElement rootElement, int level, Action<int, AutomationElement,AutomationElement> action)
        {
            // Conditions for the basic views of the subtree (content, control, and raw) 
            // are available as fields of TreeWalker, and one of these is used in the 
            // following code.
            AutomationElement elementNode = TreeWalker.ContentViewWalker.GetFirstChild(rootElement);

            while (elementNode != null)
            {
                action(level, rootElement, elementNode);
                WalkControlElements(elementNode, level + 1, action);
                elementNode = TreeWalker.ContentViewWalker.GetNextSibling(elementNode);
            }
        }
    }
}
