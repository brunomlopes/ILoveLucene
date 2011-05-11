using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Threading;

namespace ElevationHelper.Services
{
    public class ElevatedChannel<T> where T : class, IAmAlive
    {
        private T _elevatedHandler;
        private ChannelFactory<T> _channelFactory;
        private readonly string _address;

        public ElevatedChannel()
        {
            _address = Addresses.AddressForType(typeof(T));
        }

        public T GetElevatedHandler()
        {
            // TODO: refactor this. it's quite messy.
            if (!ElevationProcessExists())
            {
                StartElevationHelper();
                if (_channelFactory != null && _channelFactory.State == CommunicationState.Closed)
                {
                    _channelFactory.Close();
                }
                _channelFactory = null;
                _elevatedHandler = null;
            }
            if (_channelFactory != null && _channelFactory.State == CommunicationState.Faulted)
            {
                _channelFactory.Close();
                _channelFactory = null;
            }
            if (_channelFactory == null)
            {
                _channelFactory = new ChannelFactory<T>(new NetNamedPipeBinding(), _address);
                _elevatedHandler = null;
            }
            var clientChannel = ((IClientChannel) _elevatedHandler);
            if (clientChannel != null && clientChannel.State == CommunicationState.Faulted)
            {
                clientChannel.Close();
                _elevatedHandler = null;
            }

            try
            {
                if (_elevatedHandler != null)
                    _elevatedHandler.AmIAlive();
            }
            catch (Exception e)
            {
                _elevatedHandler = null;
            }
            T elevatedHandler = _elevatedHandler ?? (_elevatedHandler = _channelFactory.CreateChannel());

            return elevatedHandler;
        }

        public bool ElevationProcessExists()
        {
            return Process.GetProcessesByName("ILoveLucene.ElevationHelper").Any();
        }

        private static void StartElevationHelper()
        {
            var location = new FileInfo(Assembly.GetEntryAssembly().Location).Directory.FullName;
            var arguments = new ProcessStartInfo(Path.Combine(location, "ILoveLucene.ElevationHelper.exe"));
            arguments.Verb = "runas";
            Process.Start(arguments);
            Thread.Sleep(500); // wait for it to start TODO: be smarter about this
        }
    }
}