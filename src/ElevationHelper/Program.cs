using System;
using System.Diagnostics;
using System.ServiceModel;
using ElevationHelper.Services;
using ElevationHelper.Services.Infrastructure;
using ElevationHelper.Services.WindowsServices;

namespace ElevationHelper
{
    public class Program
    {
        private static readonly System.Threading.AutoResetEvent StopFlag = new System.Threading.AutoResetEvent(false);

        private static void Main(string[] args)
        {
            ServiceHost svh = OpenServiceHost(typeof (ServiceHandler), typeof (IServiceHandler));
            ServiceHost stop = OpenServiceHost(new StopElevationHelper(StopFlag), typeof(IStopTheElevationHelper));


            var readyChannelFactory = new ChannelFactory<IElevationHelperReady>(new NetNamedPipeBinding(), Addresses.AddressForType(typeof(IElevationHelperReady)));
            Debug.Write("Opening channel to main process");

            var channel = readyChannelFactory.CreateChannel();

            Debug.Write("Ready");
            channel.Ready();
            StopFlag.WaitOne();

            svh.Close();
            stop.Close();
        }

        private static ServiceHost OpenServiceHost(Type type, Type contract)
        {
            var svh = new ServiceHost(type);
            svh.AddServiceEndpoint(contract, new NetNamedPipeBinding(), Addresses.AddressForType(contract));
            svh.Open();
            return svh;
        }

        private static ServiceHost OpenServiceHost(object singletonInstance, Type contract)
        {
            var svh = new ServiceHost(singletonInstance);
            svh.AddServiceEndpoint(contract, new NetNamedPipeBinding(), Addresses.AddressForType(contract));            
            svh.Open();
            return svh;
        }
    }
}
