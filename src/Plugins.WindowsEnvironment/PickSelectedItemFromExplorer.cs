using System;
using System.ComponentModel.Composition;
using System.IO;
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
            FileInfo fileInfo = WindowsExplorer.GetTopSelectedFileFromExplorer();
            if (fileInfo == null)
                return null;
            return new FileInfoItem(fileInfo);
        }
    }
}