using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using ElevationHelper.Services;

namespace ElevationHelper
{
    public class Program
    {
        private static System.Threading.AutoResetEvent stopFlag = new System.Threading.AutoResetEvent(false);

        private static void Main(string[] args)
        {
            ServiceHost svh = OpenServiceHost(typeof (ServiceHandler), typeof (IServiceHandler), Addresses.Services);
            ServiceHost stop = OpenServiceHost(new StopElevationHelper(stopFlag), typeof(IStopTheElevationHelper), Addresses.StopElevationHelper);

            stopFlag.WaitOne();

            svh.Close();
            stop.Close();
        }

        private static ServiceHost OpenServiceHost(Type type, Type contract, string address)
        {
            var svh = new ServiceHost(type);
            svh.AddServiceEndpoint(contract, new NetNamedPipeBinding(), address);
            svh.Open();
            return svh;
        }

        private static ServiceHost OpenServiceHost(object singletonInstance, Type contract, string address)
        {
            var svh = new ServiceHost(singletonInstance);
            svh.AddServiceEndpoint(contract, new NetNamedPipeBinding(), address);            
            svh.Open();
            return svh;
        }
    }
}
