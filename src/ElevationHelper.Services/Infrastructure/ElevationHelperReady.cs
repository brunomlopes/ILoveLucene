using System;
using System.ServiceModel;
using System.Threading;

namespace ElevationHelper.Services.Infrastructure
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ElevationHelperReady : IElevationHelperReady
    {
        private static AutoResetEvent _readyFlag = new AutoResetEvent(false);
        private static ServiceHost _host;

        public static void EnsureHostExists()
        {
            if (_host == null)
            {
                _host = new ServiceHost(new ElevationHelperReady());
                _host.Faulted += (sender, e) =>
                                     {
                                         _host.Close();
                                         _host = null;
                                     };
                _host.Closed += (sender, e) => { _host = null; };
                _host.AddServiceEndpoint(typeof (IElevationHelperReady), new NetNamedPipeBinding(),
                                         Addresses.AddressForType(typeof (IElevationHelperReady)));
                _host.Open();
            }
        }

        public void Ready()
        {
            _readyFlag.Set();
        }

        public static void Wait()
        {
            EnsureHostExists();
            _readyFlag.WaitOne(TimeSpan.FromMinutes(1));
        }
    }
}