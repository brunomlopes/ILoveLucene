using System;
using System.Linq;
using System.ServiceModel;
using System.ServiceProcess;

namespace ElevationHelper.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, IncludeExceptionDetailInFaults = true)]
    public class ServiceHandler : IServiceHandler
    {
        public void StartService(string serviceName)
        {
            var service = GetService(serviceName);
            if (service.Status == ServiceControllerStatus.Paused)
                service.Continue();
            else if (service.Status == ServiceControllerStatus.Stopped)
                service.Start();
        }

        public void StopService(string serviceName)
        {
            GetService(serviceName).Stop();
        }

        public void RestartService(string serviceName)
        {
            var service = GetService(serviceName);
            service.Stop();
            service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(20));
            service.Start();
        }

        private ServiceController GetService(string serviceName)
        {
            return ServiceController.GetServices().Single(s => s.ServiceName == serviceName);
        }
    }
}