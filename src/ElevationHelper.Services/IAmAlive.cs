using System.ServiceModel;

namespace ElevationHelper.Services
{
    [ServiceContract]
    public interface IAmAlive
    {
        /// <summary>
        /// Hackiest method ever. This is for the client to try and call 
        /// If it fails, restart the pipe connection
        /// </summary>
        [OperationContract]
        void AmIAlive();
    }
}