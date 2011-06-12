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
        private UpdateManagerAdapter _updateManagerAdapter;

        public UpdateManagerAdapter UpdateManagerAdapter
        {
            get { return _updateManagerAdapter; }
        }

        public AutoUpdateModule()
        {
            UpdateManager updManager = UpdateManager.Instance;

            updManager.UpdateFeedReader = new ZippedAppcastReader();

            _updateManagerAdapter = new UpdateManagerAdapter(updManager);
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(UpdateManagerAdapter)
                .AsSelf();
        }
    }
}