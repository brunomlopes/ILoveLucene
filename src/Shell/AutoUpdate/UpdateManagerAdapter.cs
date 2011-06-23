using System;
using System.ComponentModel.Composition;
using NAppUpdate.Framework;
using NAppUpdate.Framework.Sources;

namespace ILoveLucene.AutoUpdate
{
    public class UpdateManagerAdapter : IPartImportsSatisfiedNotification
    {
        private readonly UpdateManager _updateManager;

        public event EventHandler<EventArgs> UpdatesAvailable;
        public event EventHandler<EventArgs> UpdatesReady;

        [Import]
        public AutoUpdateConfiguration Configuration { get; set; }

        public UpdateManagerAdapter()
        {
            UpdatesAvailable += (sender, e) => { };
            UpdatesReady += (sender, e) => { };
            State = UpdateManager.UpdateProcessState.NotChecked;
            _updateManager = UpdateManager.Instance;
        }

        public void OnImportsSatisfied()
        {
            _updateManager.UpdateFeedReader = new ZippedAppcastReader();
            _updateManager.UpdateSource = new SimpleWebSource(Configuration.AppcastFeedUrl);
        }

        public void CheckForUpdates()
        {
            if(!Configuration.CheckForUpdates)
            {
                State = UpdateManager.UpdateProcessState.NotChecked;
                return;
            }
            _updateManager.CheckForUpdateAsync(i =>
                                                   {
                                                       State = UpdateManager.UpdateProcessState.Checked;
                                                       HaveUpdatesAvailable = i > 0;
                                                       if (HaveUpdatesAvailable)
                                                       {
                                                           UpdatesAvailable(this, new EventArgs());
                                                       }
                                                   });
        }

        public void PrepareUpdates()
        {
            if(State != UpdateManager.UpdateProcessState.Checked)
            {
                throw new Exception("Invalid state. We need to check before preparing");
            }
            _updateManager.PrepareUpdatesAsync(prepared =>
                                                   {
                                                       State = UpdateManager.UpdateProcessState.Prepared;
                                                       UpdatesReady(this, new EventArgs());
                                                   });
        }

        /// <summary>
        /// This will restart the application
        /// </summary>
        public void ApplyUpdates()
        {
            _updateManager.ApplyUpdates(true);
        }

        public bool HaveUpdatesAvailable { get; private set; }

        public UpdateManager.UpdateProcessState State { get; private set; }

        public string LatestError
        {
            get { return _updateManager.LatestError; }
            set { _updateManager.LatestError = value; }
        }

        public bool IsWorking
        {
            get { return _updateManager.IsWorking; }
        }
    }
}