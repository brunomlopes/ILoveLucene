using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.ServiceModel;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using Core.Abstractions;
using System.Linq;
using ElevationHelper.Services;

namespace Plugins.Services
{
    // TODO: The actions require elevation...
    [Export(typeof (IItemSource))]
    public class ServicesSource : BaseItemSource
    {
        private static IServiceHandler _elevatedHandler;
        private static ChannelFactory<IServiceHandler> _scf;

        public override Task<IEnumerable<object>> GetItems()
        {
            return Task.Factory.StartNew(() => ServiceController.GetServices().Cast<object>());
        }

        public static IServiceHandler GetElevatedHandler()
        {
            // TODO: refactor this. it's quite messy.
            if (!Process.GetProcessesByName("ILoveLucene.ElevationHelper").Any())
            {
                StartElevationHelper();
                if(_scf != null && _scf.State == CommunicationState.Closed)
                {
                    _scf.Close();
                }
                _scf = null;
                _elevatedHandler = null;
            }
            if(_scf != null && _scf.State == CommunicationState.Faulted)
            {
                _scf.Close();
                _scf = null;
            }
            if(_scf == null)
            {
                _scf = new ChannelFactory<IServiceHandler>(new NetNamedPipeBinding(), Addresses.Services);
                _elevatedHandler = null;
            }
            var clientChannel = ((IClientChannel)_elevatedHandler);
            if (clientChannel != null && clientChannel.State == CommunicationState.Faulted)
            {
                clientChannel.Close();
                _elevatedHandler = null;
            }
            return _elevatedHandler ?? (_elevatedHandler = _scf.CreateChannel());
        }

        public static void StartElevationHelper()
        {
            string location = new FileInfo(Assembly.GetEntryAssembly().Location).Directory.FullName;
            var arguments = new ProcessStartInfo(Path.Combine(location, "ILoveLucene.ElevationHelper.exe"));
            arguments.Verb = "runas";
            Process.Start(arguments);
            Thread.Sleep(500); // wait for it to start TODO: be smarter about this
        }

        public override bool Persistent
        {
            get { return false; }
        }
    }
}