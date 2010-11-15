using System;
using System.Threading.Tasks;

namespace Core.Extensions
{
    public static class TaskExtensions
    {
        public static void GuardForException(this Task task, Action<Exception> callback)
        {
            task.ContinueWith(t =>
                                  {
                                      if (t.Exception != null)
                                      {
                                          callback(t.Exception);
                                      }
                                  });
        }
        
        public static void GuardForException(this Task task, Action<Task, Exception> callback)
        {
            task.ContinueWith(t =>
                                  {
                                      if (t.Exception != null)
                                      {
                                          callback(t, t.Exception);
                                      }
                                  });
        }
    }
}