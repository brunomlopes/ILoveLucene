using ElevationHelper.Services;
using ElevationHelper.Services.Infrastructure;
using Quartz;

namespace ILoveLucene.Infrastructure
{
    public class Shutdown
    {
        private readonly IScheduler _scheduler;

        public Shutdown(IScheduler scheduler)
        {
            _scheduler = scheduler;
        }

        public void Now()
        {
            _scheduler.Shutdown(false);
            var elevatedChannel = new ElevatedChannel<IStopTheElevationHelper>();
            if (elevatedChannel.ElevationProcessExists())
                elevatedChannel.GetElevatedHandler().Stop();
        }
    }
}