using System.ServiceModel;

namespace ElevationHelper.Services.Infrastructure
{
    /// <summary>
    /// Hosted by the main process and used to wait for readyness from the subprocess
    /// </summary>
    [ServiceContract]
    public interface IElevationHelperReady
    {
        [OperationContract]
        void Ready();
    }
}