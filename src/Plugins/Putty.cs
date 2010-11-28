using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Core.Abstractions;
using Microsoft.Win32;

namespace Plugins
{
    [Export(typeof (IActOnItem))]
    public class Putty : BaseActOnTypedItem<FileInfo>, ICanActOnTypedItem<FileInfo>,
                         IActOnTypedItemWithArguments<FileInfo>,
                         IActOnTypedItemWithAutoCompletedArguments<FileInfo>
    {
        private readonly string[] _sessionNames;

        public Putty()
        {
            var sessionsKey = Registry.CurrentUser.OpenSubKey(@"Software\SimonTatham\PuTTY\Sessions");
            if (sessionsKey == null) _sessionNames = new string[] {};
            else _sessionNames = sessionsKey.GetSubKeyNames();
        }

        public override void ActOn(ITypedItem<FileInfo> item)
        {
            Process.Start(item.Item.FullName);
        }

        public ArgumentAutoCompletionResult AutoCompleteArguments(ITypedItem<FileInfo> item, string arguments)
        {
            if (string.IsNullOrWhiteSpace(arguments))
            {
                return ArgumentAutoCompletionResult.NoResult(arguments);
            }
            return ArgumentAutoCompletionResult.OrderedResult(arguments,
                                                              _sessionNames.Where(s => s.Contains(arguments)));
        }

        public void ActOn(ITypedItem<FileInfo> item, string arguments)
        {
            if (_sessionNames.Contains(arguments))
            {
                arguments = "-load " + arguments;
            }
            Process.Start("putty", arguments);
        }

        public override string Text
        {
            get { return "Launch putty session"; }
        }

        public string Description
        {
            get { return "Putty launcher"; }
        }

        public bool CanActOn(ITypedItem<FileInfo> item)
        {
            return Path.GetFileNameWithoutExtension(item.Item.Name)
                .Equals("putty", StringComparison.InvariantCultureIgnoreCase);
        }
    }
}