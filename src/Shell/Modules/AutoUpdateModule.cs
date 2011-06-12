using System;
using System.IO;
using Autofac.Core;
using ILoveLucene.AutoUpdate;
using NAppUpdate.Framework;
using Autofac;

namespace ILoveLucene.Modules
{
    public class AutoUpdateModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            UpdateManager updManager = UpdateManager.Instance;

            updManager.UpdateFeedReader = new ZippedAppcastReader();
            updManager.UpdateSource = new NAppUpdate.Framework.Sources.SimpleWebSource("file:///d:/documents/dev/ILoveLucene/appcast.xml");

            builder.RegisterInstance(updManager)
                .AsSelf();
        }
    }
}