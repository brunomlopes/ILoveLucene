using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using Core.Abstractions;

namespace Plugins
{
    [Export(typeof (IItem))]
    public class Browse : ICommandWithArguments
    {
        public string Text
        {
            get { return "Browse"; }
        }

        public string Description
        {
            get { return "Launch a browser"; }
        }

        public void Execute()
        {
            // NO-OP
        }

        public void Execute(string arguments)
        {
            Uri uri;
            if (Uri.TryCreate(arguments, UriKind.Absolute, out uri))
            {
                Process.Start(uri.ToString());
            }
            else
            {
                throw new InvalidOperationException("'{0}' doesn't seem like an url");
            }
        }
    }
}