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
        private readonly string _appcastUrl;

        public AutoUpdateModule(string appcastUrl)
        {
            _appcastUrl = appcastUrl;
        }

        protected override void Load(ContainerBuilder builder)
        {
            UpdateManager updManager = UpdateManager.Instance;

            updManager.UpdateFeedReader = new ZippedAppcastReader();
            updManager.UpdateSource = new NAppUpdate.Framework.Sources.SimpleWebSource(_appcastUrl);

            builder.RegisterInstance(updManager)
                .AsSelf();
        }
    }
}