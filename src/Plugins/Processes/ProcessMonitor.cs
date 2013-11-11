using System;
using System.Collections.Concurrent;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Threading.Tasks;
using Core.Abstractions;
using System.Management;
using Core.Lucene;
using WMI.Win32;

namespace Plugins.Processes
{
    public class ProcessMonitor : IStartupTask, IDisposable
    {
        private readonly SourceStorageFactory _sourceStorageFactory;
        private readonly ProcessSource _source;

        private readonly ConcurrentDictionary<uint, Process> _cachedProcesses = new ConcurrentDictionary<uint, Process>();
        private ManagementEventWatcher _modificationWatcher;
        private ManagementEventWatcher _creationDeletionWatcher;
        private ManagementEventWatcher _creationWatcher;
        private ManagementEventWatcher _deletionWatcher;
        private ForegroundTracker _windowTracker;

        [Import]
        public ILog Log { get; set; }


        // WMI WQL process query strings
        private const string CreationEvent = @"SELECT * FROM 
__InstanceCreationEvent WITHIN 5 WHERE TargetInstance ISA 'Win32_Process' ";        
        private const string DeletionEvent = @"SELECT * FROM 
__InstanceDeletionEvent WITHIN 5 WHERE TargetInstance ISA 'Win32_Process' ";

        public ProcessMonitor(SourceStorageFactory sourceStorageFactory)
        {
            _sourceStorageFactory = sourceStorageFactory;
            _source = new ProcessSource();
        }


        private void watcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            string eventType = e.NewEvent.ClassPath.ClassName;
            Win32_Process proc = new
                Win32_Process(e.NewEvent["TargetInstance"] as ManagementBaseObject);

            switch (eventType)
            {
                case "__InstanceCreationEvent":
                    ProcessCreated(proc);
                    break;
                case "__InstanceDeletionEvent":
                    ProcessDeleted(proc);
                    break;
                case "__InstanceModificationEvent":
                    ProcessModified(proc);
                    break;
            }
        }

        private void ProcessModified(Win32_Process proc)
        {
            ProcessCreated(proc);
        }

        private void ProcessDeleted(Win32_Process proc)
        {
            Log.Debug("Process {0} ({1}) deleted", proc.Name, proc.ProcessId);
            Process process;
            var storage = _sourceStorageFactory.SourceStorageFor(_source.Id);
            if (_cachedProcesses.TryRemove(proc.ProcessId, out process))
            {
                storage.RemoveItems(_source, process);
            }
            else
            {
                storage.RemoveItems(_source, proc);
            }

        }

        private void ProcessCreated(Win32_Process proc)
        {
            var processId = proc.ProcessId;
            ReIndexProcess((int)processId);
        }

        private void ReIndexProcess(int processId)
        {
            Process process;
            string previousTitle = null;
            if (_cachedProcesses.TryGetValue((uint)processId, out process))
            {
                previousTitle = process.MainWindowTitle;
            }
            try
            {
                process = Process.GetProcessById((int) processId);
            }
            catch (ArgumentException)
            {
                // process quit? so will I
                return;
            }
            if (previousTitle == process.MainWindowTitle) return;
            Log.Debug("Updating process {0} ({1})", process, process.Id);

            var storage = _sourceStorageFactory.SourceStorageFor(_source.Id);
            storage.AppendItems(_source, process);
        }


        [Import]
        public IOnUiThread OnUiThread { get; set; }

        public void Execute()
        {
            Init();
            OnUiThread.Execute(() =>
            {
                _windowTracker = new ForegroundTracker(i => Task.Factory.StartNew(() => ReIndexProcess(i)));
                _windowTracker.Start();
            });
        }


        private void Init()
        {
            _creationWatcher = new ManagementEventWatcher(new EventQuery("WQL", CreationEvent));
            _deletionWatcher = new ManagementEventWatcher(new EventQuery("WQL", DeletionEvent));

            _creationWatcher.EventArrived += watcher_EventArrived;
            _deletionWatcher.EventArrived += watcher_EventArrived;
            
            _creationWatcher.Start();
            _deletionWatcher.Start();
        }

        public void Dispose()
        {
            if(_windowTracker != null) _windowTracker.Dispose();
            if(_creationWatcher != null) _creationWatcher.Dispose();
            if(_deletionWatcher != null) _deletionWatcher.Dispose();
        }
    }
}
