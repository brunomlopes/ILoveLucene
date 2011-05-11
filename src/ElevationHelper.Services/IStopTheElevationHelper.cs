using System.ServiceModel;

namespace ElevationHelper.Services
{
    [ServiceContract]
    public interface IStopTheElevationHelper : IAmAlive
    {
        [OperationContract]
        void Stop();
    }
}