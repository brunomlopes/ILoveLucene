using System;
using NAppUpdate.Framework;
using NAppUpdate.Framework.Sources;

namespace ILoveLucene.AutoUpdate
{
    public class UpdateManagerAdapter
    {
        private readonly UpdateManager _updateManager;

        public event EventHandler<EventArgs> UpdatesAvailable;
        public event EventHandler<EventArgs> UpdatesReady;

        public UpdateManagerAdapter(UpdateManager updateManager)
        {
            _updateManager = updateManager;
            UpdatesAvailable += (sender, e) => { };
            UpdatesReady += (sender, e) => { };
            State = UpdateManager.UpdateProcessState.NotChecked;
        }
        
        public void CheckForUpdates(IUpdateSource source)
        {
            _updateManager.CheckForUpdateAsync(source, i =>
                                                           {
                                                               State = UpdateManager.UpdateProcessState.Checked;
                                                               HaveUpdatesAvailable = i > 0;
                                                               if(HaveUpdatesAvailable)
                                                               {
                                                                   UpdatesAvailable(this, new EventArgs());
                                                               }
                                                           });
        }

        public void PrepareUpdates()
        {
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