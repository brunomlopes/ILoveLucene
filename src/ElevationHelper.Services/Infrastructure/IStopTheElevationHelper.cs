using System.ServiceModel;

namespace ElevationHelper.Services.Infrastructure
{
    [ServiceContract]
    public interface IStopTheElevationHelper : IAmAlive
    {
        [OperationContract]
        void Stop();
    }
}