using System;
using System.ServiceModel;
using System.Threading;
using ElevationHelper.Services;
using ElevationHelper.Services.Infrastructure;

namespace ElevationHelper
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class StopElevationHelper : IStopTheElevationHelper
    {
        private readonly AutoResetEvent _stopFlag;

        public StopElevationHelper(AutoResetEvent stopFlag)
        {
            _stopFlag = stopFlag;
        }

        public void Stop()
        {
            _stopFlag.Set();
        }

        public void AmIAlive()
        {
            return;
        }
    }
}