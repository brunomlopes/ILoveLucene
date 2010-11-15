using System;
using System.Threading.Tasks;

namespace Core.Extensions
{
    public static class TaskExtensions
    {
        public static Task GuardForException(this Task task, Action<Exception> callback)
        {
            return task.ContinueWith(t =>
                                  {
                                      if (t.Exception != null)
                                      {
                                          callback(t.Exception);
                                      }
                                  });
        }
        
        public static Task<T> GuardForException<T>(this Task<T> task, Action<Exception> callback)
        {
            return task.ContinueWith(t =>
                                  {
                                      if (t.Exception != null)
                                      {
                                          callback(t.Exception);
                                      }
                                      return t.Result;
                                  });
        }
        
        public static Task GuardForException(this Task task, Action<Task, Exception> callback)
        {
            return task.ContinueWith(t =>
                                  {
                                      if (t.Exception != null)
                                      {
                                          callback(t, t.Exception);
                                      }
                                  });
        }
    }
}