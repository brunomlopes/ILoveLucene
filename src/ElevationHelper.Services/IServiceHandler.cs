using System.ServiceModel;

namespace ElevationHelper.Services
{
    [ServiceContract]
    public interface IServiceHandler
    {
        [OperationContract]
        void StartService(string serviceName);

        [OperationContract]
        void StopService(string serviceName);

        [OperationContract]
        void RestartService(string serviceName);

        /// <summary>
        /// Hackiest method ever. This is for the client to try and call 
        /// If it fails, restart the pipe connection
        /// </summary>
        [OperationContract]
        void AmIAlive();
    }
}
