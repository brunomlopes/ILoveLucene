using System.ServiceModel;
using ElevationHelper.Services;

namespace ElevationHelper
{
    public class Program
    {
        private static System.Threading.AutoResetEvent stopFlag = new System.Threading.AutoResetEvent(false);

        private static void Main(string[] args)
        {
            var svh = new ServiceHost(typeof (ServiceHandler));
            svh.AddServiceEndpoint(typeof (IServiceHandler), new NetNamedPipeBinding(), Addresses.Services);

            svh.Open();

            stopFlag.WaitOne();

            svh.Close();
        }

        public static void Stop()
        {
            stopFlag.Set();
        }
    }
}
