using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using ElevationHelper.Services.Infrastructure;

namespace ElevationHelper.Services
{
    public class ElevatedChannel<T> where T : class, IAmAlive
    {
        private T _elevatedHandler;
        private ChannelFactory<T> _channelFactory;
        private readonly string _address;

        public ElevatedChannel()
        {
            _address = Addresses.AddressForType(typeof (T));
        }

        public T GetElevatedHandler()
        {
            if (!ElevationProcessExists())
            {
                StartElevationHelper();
            }
            if (_channelFactory == null)
            {
                _channelFactory = new ChannelFactory<T>(new NetNamedPipeBinding(), _address);
                _channelFactory.Closed += (sender, e) =>
                                              {
                                                  _channelFactory = null;
                                                  _elevatedHandler = null;
                                              };
                _channelFactory.Faulted += (sender, e) =>
                                               {
                                                   _channelFactory.Close();
                                                   _channelFactory = null;
                                                   _elevatedHandler = null;
                                               };
                _elevatedHandler = null;
            }

            try
            {
                if (_elevatedHandler != null)
                    _elevatedHandler.AmIAlive();
            }
            catch (Exception)
            {
                _elevatedHandler = null;
            }
            if (_elevatedHandler == null)
            {
                _elevatedHandler = _channelFactory.CreateChannel();
                ((IClientChannel) _elevatedHandler).Faulted += (sender, e) =>
                                                                   {
                                                                       ((IClientChannel) _elevatedHandler).Close();
                                                                       _elevatedHandler = null;
                                                                   };
            }
            T elevatedHandler = _elevatedHandler;

            return elevatedHandler;
        }

        public bool ElevationProcessExists()
        {
            return Process.GetProcessesByName("ILoveLucene.ElevationHelper").Any();
        }

        private static void StartElevationHelper()
        {
            ElevationHelperReady.EnsureHostExists();
            var location = new FileInfo(Assembly.GetEntryAssembly().Location).Directory.FullName;
            var arguments = new ProcessStartInfo(Path.Combine(location, "ILoveLucene.ElevationHelper.exe"));
            arguments.Verb = "runas";
            Process.Start(arguments);
            ElevationHelperReady.Wait();
        }
    }
}