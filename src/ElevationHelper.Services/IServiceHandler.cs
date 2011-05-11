using System.ServiceModel;

namespace ElevationHelper.Services
{
    [ServiceContract]
    public interface IServiceHandler : IAmAlive
    {
        [OperationContract]
        void StartService(string serviceName);

        [OperationContract]
        void StopService(string serviceName);

        [OperationContract]
        void RestartService(string serviceName);
    }
}
