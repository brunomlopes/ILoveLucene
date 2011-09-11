using System;
using System.ComponentModel.Composition;
using System.IO;
using Core.API;
using Core.Abstractions;
using Plugins.Commands;
using Plugins.Shortcuts;

namespace Plugins.WindowsEnvironment
{
    [Export(typeof(IRequest))]
    public class PickSelectedItemFromExplorer : BaseRequest
    {
        public override IItem Act()
        {
            var path = WindowsExplorer.GetTopSelectedPathFromWindowsExplorer();
            if (path == null)
            {
                throw new InvalidOperationException("No explorer window open");
            }
            if(!File.Exists(path))
            {
                throw new InvalidOperationException(string.Format("Path '{0}' is not a file.", path));
            }
            return new FileInfoItem(new FileInfo(path));
        }
    }
}