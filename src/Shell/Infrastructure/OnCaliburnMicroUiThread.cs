using System;
using System.ComponentModel.Composition;
using Core.Abstractions;

namespace ILoveLucene.Infrastructure
{
    [Export(typeof(IOnUiThread))]
    public class OnCaliburnMicroUiThread : IOnUiThread
    {
        [Import]
        public ILog Log { get; set; }

        public void Execute(Action action)
        {
            Caliburn.Micro.Execute.OnUIThread(() =>
            {
                try
                {
                    action();
                }
                catch (Exception e)
                {
                    Log.Error(e, "Error on ui thread:{0}", e.Message);
                }
            });
        }
    }
}